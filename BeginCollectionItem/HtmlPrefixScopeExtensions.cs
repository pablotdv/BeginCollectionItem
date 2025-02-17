using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.Collections.Generic;
using System.IO;

namespace HtmlHelpers.BeginCollectionItem
{
    public static class HtmlPrefixScopeExtensions
    {
        private const string IdsToReuseKey = "__htmlPrefixScopeExtensions_IdsToReuse_";

        public static IDisposable BeginCollectionItem(this IHtmlHelper html, string collectionName)
        {
            return BeginCollectionItem(html, collectionName, html.ViewContext.Writer);
        }

        public static IDisposable BeginCollectionItem(this IHtmlHelper html, string collectionName, TextWriter writer)
        {
            var idsToReuse = GetIdsToReuse(html.ViewContext.HttpContext, collectionName);
            var itemIndex = idsToReuse.Count > 0 ? idsToReuse.Dequeue() : Guid.NewGuid().ToString();

            // autocomplete="off" is needed to work around a very annoying Chrome behaviour
            // whereby it reuses old values after the user clicks "Back", which causes the
            // xyz.index and xyz[...] values to get out of sync.
            writer.WriteLine(
                "<input type=\"hidden\" name=\"{0}.index\" autocomplete=\"off\" value=\"{1}\" />",
                collectionName, html.Encode(itemIndex));

            return BeginHtmlFieldPrefixScope(html, string.Format("{0}[{1}]", collectionName, itemIndex));
        }

        public static IDisposable BeginHtmlFieldPrefixScope(this IHtmlHelper html, string htmlFieldPrefix)
        {
            return new HtmlFieldPrefixScope(html.ViewData.TemplateInfo, htmlFieldPrefix);
        }

        private static Queue<string> GetIdsToReuse(Microsoft.AspNetCore.Http.HttpContext httpContext, string collectionName)
        {
            // We need to use the same sequence of IDs following a server-side validation failure,
            // otherwise the framework won't render the validation error messages next to each item.
            var key = IdsToReuseKey + collectionName;

            var hasKey = httpContext.Items.ContainsKey(key);

            var queue = hasKey ? (Queue<string>)httpContext.Items[key] : null;
            if (queue == null)
            {
                httpContext.Items[key] = queue = new Queue<string>();
                var previouslyUsedIds = httpContext.Request.Form[collectionName + ".index"];
                if (!string.IsNullOrEmpty(previouslyUsedIds))
                    foreach (var previouslyUsedId in previouslyUsedIds.ToString().Split(','))
                        queue.Enqueue(previouslyUsedId);
            }
            return queue;
        }

        internal class HtmlFieldPrefixScope : IDisposable
        {
            internal readonly TemplateInfo TemplateInfo;
            internal readonly string PreviousHtmlFieldPrefix;

            public HtmlFieldPrefixScope(TemplateInfo templateInfo, string htmlFieldPrefix)
            {
                TemplateInfo = templateInfo;

                PreviousHtmlFieldPrefix = TemplateInfo.HtmlFieldPrefix;
                TemplateInfo.HtmlFieldPrefix = htmlFieldPrefix;
            }

            public void Dispose()
            {
                TemplateInfo.HtmlFieldPrefix = PreviousHtmlFieldPrefix;
            }
        }
    }
}