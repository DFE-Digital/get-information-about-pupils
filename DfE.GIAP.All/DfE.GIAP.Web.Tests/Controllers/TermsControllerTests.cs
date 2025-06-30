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
    [Trait("Category", "Terms Controller Unit Tests")]
    public class TermsControllerTests : IClassFixture<TermsResultsFake>
    {
        private readonly TermsResultsFake _termsResultsFake;
        private readonly IContentService _mockContentService = Substitute.For<IContentService>();

        public TermsControllerTests(TermsResultsFake termsResultsFake)
        {
            _termsResultsFake = termsResultsFake;
        }

        [Fact]
        public async Task TermsController_Index_Should_Return_Termsdata()
        {
            // Arrange
            var model = _termsResultsFake.GetTermsDetails();
            _mockContentService.GetContent(DocumentType.TermOfUse).ReturnsForAnyArgs(model.Response);

            var controller = GetTermsController();

            // Act
            var result = await controller.Index().ConfigureAwait(false);

            // Assert
            var viewResult = Assert.IsAssignableFrom<ViewResult>(result);
            var viewModel = viewResult.Model as TermsOfUseViewModel;
            Assert.Equal(viewModel.Response.Title, model.Response.Title);
            Assert.Equal(viewModel.Response.Body, model.Response.Body);
        }

        private TermsController GetTermsController()
        {
            return new TermsController(_mockContentService);
        }
    }
}
