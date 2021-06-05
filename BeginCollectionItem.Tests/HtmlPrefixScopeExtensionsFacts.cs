using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace HtmlHelpers.BeginCollectionItem.Tests
{

    public class TheBeginCollectionItemMethod
    {
        private readonly Mock<IHtmlHelper> _htmlHelper;

        public TheBeginCollectionItemMethod()
        {
            _htmlHelper = new Mock<IHtmlHelper>();
        }

        [Fact]
        public void WritesCollectionIndexHiddenInput_WhenThereIsNothingInRequestData()
        {
            const string collectionName = "CollectionName";
            var index0 = Guid.NewGuid();
            var index1 = Guid.NewGuid();
            var indexes = string.Format("{0},{1}", index0, index1);
            var httpContext = new Mock<HttpContext>();
            var httpContextItems = new Dictionary<object, object>();
            httpContext.Setup(p => p.Items).Returns(httpContextItems);

            var httpRequest = new Mock<HttpRequest>();
            httpContext.Setup(p => p.Request).Returns(httpRequest.Object);

            httpRequest.SetupGet(i => i.Form[It.Is<string>(s => s == string.Format("{0}.index", collectionName))])
                .Returns(indexes);

            var viewContext = new ViewContext();
            var writer = new StringWriter();
            viewContext.Writer = writer;

            _htmlHelper.SetupGet(x => x.ViewContext).Returns(viewContext);
            _htmlHelper.SetupGet(x => x.ViewData).Returns(new ViewDataDictionary(new Mock<IModelMetadataProvider>().Object, new ModelStateDictionary()));
            viewContext.HttpContext = httpContext.Object;

            using (var result = HtmlPrefixScopeExtensions.BeginCollectionItem(_htmlHelper.Object, collectionName))
            {
                Assert.NotNull(result);
            }

            var text = writer.ToString();
            Assert.NotNull(text);
            Assert.NotEmpty(text);
            Assert.StartsWith(string.Format(
                @"<input type=""hidden"" name=""{0}.index"" autocomplete=""off"" value=""",
                    collectionName), text);
            Assert.Contains(@""" />", text);
        }

        [Fact]
        public void WritesExpectedCollectionIndexHiddenInput_WhenThereIsAnIndexInRequestData()
        {
            const string collectionName = "CollectionName";
            var index0 = Guid.NewGuid();
            var index1 = Guid.NewGuid();
            var indexes = string.Format("{0},{1}", index0, index1);
            var httpContext = new Mock<HttpContext>();
            var httpContextItems = new Dictionary<object, object>
            {
                { "__htmlPrefixScopeExtensions_IdsToReuse_CollectionName", "" }
            };
            httpContext.SetupGet(p => p.Items).Returns(httpContextItems);

            var httpRequest = new Mock<HttpRequest>();
            httpRequest.SetupGet(i => i.Form[It.Is<string>(s => s == string.Format("{0}.index", collectionName))])
                .Returns(indexes);
            httpContext.SetupGet(p => p.Request).Returns(httpRequest.Object);

            var viewContext = new ViewContext
            {
                HttpContext = httpContext.Object,
                Writer = new StringWriter()
            };

            _htmlHelper.SetupGet(x => x.ViewContext).Returns(viewContext);

            using (var result = HtmlPrefixScopeExtensions.BeginCollectionItem(_htmlHelper.Object, collectionName))
            {
                Assert.NotNull(result);
            }

            var text = viewContext.Writer.ToString();
            Assert.NotNull(text);
            Assert.NotEmpty(text);
            Assert.StartsWith(string.Format(
                @"<input type=""hidden"" name=""{0}.index"" autocomplete=""off"" value=""{1}"" />",
                    collectionName, index0), text);
        }
    }
    /*
    [TestFixture, TestClass]
    public class TheBeginCollectionItemMethodOverload
    {
        [Test, TestMethod]
        public void WritesCollectionIndexHiddenInput_WhenThereIsNothingInRequestData()
        {
            const string collectionName = "CollectionName";
            var httpContext = new Mock<HttpContextBase>();
            var httpContextItems = new Dictionary<string, object>();
            httpContext.Setup(p => p.Items).Returns(httpContextItems);

            var httpRequest = new Mock<HttpRequestBase>();
            httpContext.Setup(p => p.Request).Returns(httpRequest.Object);

            var viewContext = new ViewContext();
            var writer = new StringWriter();
            viewContext.Writer = writer;

            var html = new HtmlHelper(viewContext, new FakeViewDataContainer());
            viewContext.HttpContext = httpContext.Object;

            using (var result = html.BeginCollectionItem(collectionName, html.ViewContext.Writer))
            {
                result.ShouldNotBeNull();
            }

            var text = writer.ToString();
            text.ShouldNotBeNull();
            text.ShouldNotBeEmpty();
            text.ShouldStartWith(string.Format(
                @"<input type=""hidden"" name=""{0}.index"" autocomplete=""off"" value=""",
                    collectionName));
            text.ShouldContain(@""" />");
        }

        [Test, TestMethod]
        public void WritesExpectedCollectionIndexHiddenInput_WhenThereIsAnIndexInRequestData()
        {
            const string collectionName = "CollectionName";
            var index0 = Guid.NewGuid();
            var index1 = Guid.NewGuid();
            var indexes = string.Format("{0},{1}", index0, index1);
            var httpContext = new Mock<HttpContextBase>();
            var httpContextItems = new Dictionary<string, object>();
            httpContext.Setup(p => p.Items).Returns(httpContextItems);

            var httpRequest = new Mock<HttpRequestBase>();
            httpRequest.Setup(i => i[It.Is<string>(s => s == string.Format("{0}.index", collectionName))])
                .Returns(indexes);
            httpContext.Setup(p => p.Request).Returns(httpRequest.Object);

            var viewContext = new ViewContext();
            var writer = new StringWriter();
            viewContext.Writer = writer;

            var html = new HtmlHelper(viewContext, new FakeViewDataContainer());
            viewContext.HttpContext = httpContext.Object;

            using (var result = html.BeginCollectionItem(collectionName, html.ViewContext.Writer))
            {
                result.ShouldNotBeNull();
            }

            var text = writer.ToString();
            text.ShouldNotBeNull();
            text.ShouldNotBeEmpty();
            text.ShouldStartWith(string.Format(
                @"<input type=""hidden"" name=""{0}.index"" autocomplete=""off"" value=""{1}"" />",
                    collectionName, index0));
        }
    }

    [TestFixture, TestClass]
    public class TheBeginHtmlFieldPrefixScopeMethod
    {
        [Test, TestMethod]
        public void Returns_IDisposable()
        {
            var viewContext = new ViewContext();
            var html = new HtmlHelper(viewContext, new FakeViewDataContainer());

            using (var result = html.BeginHtmlFieldPrefixScope(string.Empty)
                as HtmlPrefixScopeExtensions.HtmlFieldPrefixScope)
            {
                result.ShouldNotBeNull();
                result.ShouldImplement<IDisposable>();
            }
        }

        [Test, TestMethod]
        public void Wraps_HtmlHelper_ViewData_TemplateInfo()
        {
            var viewContext = new ViewContext();
            var html = new HtmlHelper(viewContext, new FakeViewDataContainer());

            using (var result = html.BeginHtmlFieldPrefixScope(string.Empty)
                as HtmlPrefixScopeExtensions.HtmlFieldPrefixScope)
            {
                result.ShouldNotBeNull();
                // ReSharper disable PossibleNullReferenceException
                result.TemplateInfo.ShouldNotBeNull();
                // ReSharper restore PossibleNullReferenceException
                result.TemplateInfo.ShouldEqual(html.ViewData.TemplateInfo);

            }
        }

        [Test, TestMethod]
        public void Changes_HtmlHelper_ViewData_TemplateInfo_HtmlFieldPrefix_WhenUsed()
        {
            const string nextFieldPrefix = "InnerItems";
            const string prevFieldPrefix = "OuterItems";
            var viewContext = new ViewContext();
            var html = new HtmlHelper(viewContext, new FakeViewDataContainer());
            html.ViewData.TemplateInfo.HtmlFieldPrefix = prevFieldPrefix;

            using (var result = html.BeginHtmlFieldPrefixScope(nextFieldPrefix)
                as HtmlPrefixScopeExtensions.HtmlFieldPrefixScope)
            {
                result.ShouldNotBeNull();
                // ReSharper disable PossibleNullReferenceException
                result.PreviousHtmlFieldPrefix.ShouldNotBeNull();
                // ReSharper restore PossibleNullReferenceException
                result.PreviousHtmlFieldPrefix.ShouldEqual(prevFieldPrefix);
                html.ViewData.TemplateInfo.HtmlFieldPrefix.ShouldEqual(nextFieldPrefix);
            }
        }

        [Test, TestMethod]
        public void Restores_HtmlHelper_ViewData_TemplateInfo_HtmlFieldPrefix_WhenDisposed()
        {
            const string nextFieldPrefix = "InnerItems";
            const string prevFieldPrefix = "OuterItems";
            var viewContext = new ViewContext();
            var html = new HtmlHelper(viewContext, new FakeViewDataContainer());
            html.ViewData.TemplateInfo.HtmlFieldPrefix = prevFieldPrefix;

            using (var result = html.BeginHtmlFieldPrefixScope(nextFieldPrefix)
                as HtmlPrefixScopeExtensions.HtmlFieldPrefixScope)
            {
                result.ShouldNotBeNull();
            }
            html.ViewData.TemplateInfo.HtmlFieldPrefix.ShouldEqual(prevFieldPrefix);
        }
    }

    public class FakeViewDataContainer : IViewDataContainer
    {
        private ViewDataDictionary _viewData = new ViewDataDictionary();
        public ViewDataDictionary ViewData
        {
            get { return _viewData; }
            set { _viewData = value; }
        }
    }
    */

}