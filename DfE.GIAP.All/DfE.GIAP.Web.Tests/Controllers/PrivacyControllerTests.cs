using DfE.GIAP.Common.Enums;
using DfE.GIAP.Service.Content;
using DfE.GIAP.Web.Controllers;
using DfE.GIAP.Web.Tests.TestDoubles;
using DfE.GIAP.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Threading.Tasks;
using Xunit;

namespace DfE.GIAP.Web.Tests.Controllers
{
    [Trait("Category", "Privacy Controller Unit Tests")]
    public class PrivacyControllerTests : IClassFixture<PrivacyResultsFake>
    {
        private readonly PrivacyResultsFake _privacyResultsFake;
        private readonly IContentService _mockContentService = Substitute.For<IContentService>();
        public PrivacyControllerTests(PrivacyResultsFake privacyResultsFake)
        {
            _privacyResultsFake = privacyResultsFake;
        }

        [Fact]
        public async Task PrivacyController_Index_Should_Return_Termsdata()
        {
            // Arrange
            var model = _privacyResultsFake.GetPrivacyDetails();
            _mockContentService.GetContent(DocumentType.PrivacyNotice).ReturnsForAnyArgs(model.Response);

            var controller = GetPrivacyController();

            // Act
            var result = await controller.Index().ConfigureAwait(false);

            // Assert
            var viewResult = Assert.IsAssignableFrom<ViewResult>(result);
            var viewModel = viewResult.Model as PrivacyViewModel;
            Assert.Equal(viewModel.Response.Title, model.Response.Title);
            Assert.Equal(viewModel.Response.Body, model.Response.Body);
        }

        private PrivacyController GetPrivacyController()
        {
            return new PrivacyController(_mockContentService);
        }
    }
}
