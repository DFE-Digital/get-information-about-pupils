﻿using DfE.GIAP.Common.Enums;
using DfE.GIAP.Service.Content;
using DfE.GIAP.Web.Controllers;
using DfE.GIAP.Web.Tests.FakeData;
using DfE.GIAP.Web.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using NSubstitute;
using System.Threading.Tasks;
using Xunit;

namespace DfE.GIAP.Web.Tests.Controllers
{
    [Trait("Category", "Accessibility Controller Unit Tests")]
    public class AccessibilityControllerTests : IClassFixture<AccessibilityResultsFake>
    {
        private readonly AccessibilityResultsFake _accessibilityResultsFake;
        private readonly IContentService _mockContentService = Substitute.For<IContentService>();

        public AccessibilityControllerTests(AccessibilityResultsFake accessibilityResultsFake)
        {
            _accessibilityResultsFake = accessibilityResultsFake;
        }

        [Fact]
        public async Task AccessibilityController_Index_Should_Return_Termsdata()
        {
            // Arrange

            var model = _accessibilityResultsFake.GetAccessibilityDetails();
            _mockContentService.GetContent(DocumentType.Accessibility).ReturnsForAnyArgs(_accessibilityResultsFake.GetCommonResponseBody());

            var controller = GetAccessibilityController();

            // Act
            var result = await controller.Index().ConfigureAwait(false);

            // Assert
            var viewResult = Assert.IsAssignableFrom<ViewResult>(result);
            var viewModel = viewResult.Model as AccessibilityViewModel;
            Assert.Equal(viewModel.Response.Title, model.Response.Title);
            Assert.Equal(viewModel.Response.Body, model.Response.Body);
        }

        [Fact]
        public async Task AccessibilityController_Report_Should_Return_Termsdata()
        {
            // Arrange
            var model = _accessibilityResultsFake.GetAccessibilityDetails();
            _mockContentService.GetContent(DocumentType.AccessibilityReport).ReturnsForAnyArgs(_accessibilityResultsFake.GetCommonResponseBody());

            var controller = GetAccessibilityController();

            // Act
            var result = await controller.Report().ConfigureAwait(false);

            // Assert
            var viewResult = Assert.IsAssignableFrom<ViewResult>(result);
            var viewModel = viewResult.Model as AccessibilityViewModel;
            Assert.Equal(viewModel.Response.Title, model.Response.Title);
            Assert.Equal(viewModel.Response.Body, model.Response.Body);
        }

        private AccessibilityController GetAccessibilityController()
        {
            ITempDataProvider tempDataProvider = Mock.Of<ITempDataProvider>();
            TempDataDictionaryFactory tempDataDictionaryFactory = new TempDataDictionaryFactory(tempDataProvider);
            ITempDataDictionary tempData = tempDataDictionaryFactory.GetTempData(new DefaultHttpContext());

            return new AccessibilityController(_mockContentService) { TempData = tempData };
        }
    }
}
