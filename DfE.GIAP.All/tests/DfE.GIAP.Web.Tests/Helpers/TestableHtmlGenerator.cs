using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.WebEncoders.Testing;
using Moq;

namespace DfE.GIAP.Web.Tests.Helpers
{
    public class TestableHtmlGenerator : DefaultHtmlGenerator
    {
        private IDictionary<string, object> _validationAttributes;

        public TestableHtmlGenerator(IModelMetadataProvider metadataProvider)
            : this(metadataProvider, Mock.Of<IUrlHelper>())
        {
        }

        public TestableHtmlGenerator(IModelMetadataProvider metadataProvider, IUrlHelper urlHelper)
            : this(
                  metadataProvider,
                  GetOptions(),
                  urlHelper,
                  validationAttributes: new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase))
        {
        }

        public TestableHtmlGenerator(
            IModelMetadataProvider metadataProvider,
            IOptions<MvcViewOptions> options,
            IUrlHelper urlHelper,
            IDictionary<string, object> validationAttributes)
            : base(
                  Mock.Of<IAntiforgery>(),
                  options,
                  metadataProvider,
                  CreateUrlHelperFactory(urlHelper),
                  new HtmlTestEncoder(),
                  new DefaultValidationHtmlAttributeProvider(options, metadataProvider, new ClientValidatorCache()))
        {
            _validationAttributes = validationAttributes;
        }

        public IDictionary<string, object> ValidationAttributes
        {
            get { return _validationAttributes; }
        }

        public static ViewContext GetViewContext(
            object model,
            IHtmlGenerator htmlGenerator,
            IModelMetadataProvider metadataProvider)
        {
            return GetViewContext(model, htmlGenerator, metadataProvider, modelState: new ModelStateDictionary());
        }

        public static ViewContext GetViewContext(
            object model,
            IHtmlGenerator htmlGenerator,
            IModelMetadataProvider metadataProvider,
            ModelStateDictionary modelState)
        {
            var actionContext = new ActionContext(
                new DefaultHttpContext(),
                new RouteData(),
                new ActionDescriptor(),
                modelState);
            var viewData = new ViewDataDictionary(metadataProvider, modelState)
            {
                Model = model,
            };
            var viewContext = new ViewContext(
                actionContext,
                Mock.Of<IView>(),
                viewData,
                Mock.Of<ITempDataDictionary>(),
                TextWriter.Null,
                new HtmlHelperOptions());

            return viewContext;
        }

        private static IOptions<MvcViewOptions> GetOptions()
        {
            var mockOptions = new Mock<IOptions<MvcViewOptions>>();
            mockOptions
                .SetupGet(options => options.Value)
                .Returns(new MvcViewOptions());

            return mockOptions.Object;
        }

        private static IUrlHelperFactory CreateUrlHelperFactory(IUrlHelper urlHelper)
        {
            var factory = new Mock<IUrlHelperFactory>();
            factory
                .Setup(f => f.GetUrlHelper(It.IsAny<ActionContext>()))
                .Returns(urlHelper);

            return factory.Object;
        }
    }
}