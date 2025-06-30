using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Service.Download;
using DfE.GIAP.Service.Content;
using DfE.GIAP.Web.Controllers;
using DfE.GIAP.Web.Tests.TestDoubles;
using DfE.GIAP.Web.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using NSubstitute;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using DfE.GIAP.Common.Enums;

namespace DfE.GIAP.Web.Tests.Controllers
{
    [Trait("Category", "Glossary Controller Unit Tests")]
    public class GlossaryControllerTests : IClassFixture<GlossaryResultsFake>
    {
        private readonly GlossaryResultsFake _GlossaryResultsFake;
        private readonly IContentService _mockContentService = Substitute.For<IContentService>();
        private readonly IDownloadService _mockDownloadService = Substitute.For<IDownloadService>();

        public GlossaryControllerTests(GlossaryResultsFake GlossaryResultsFake)
        {
            _GlossaryResultsFake = GlossaryResultsFake;
        }

        [Fact]
        public async Task GlossaryController_Index_Should_Return_Termsdata()
        {
            // Arrange
            var user = new UserClaimsPrincipalFake().GetUserClaimsPrincipal();
            var model = _GlossaryResultsFake.GetGlossaryDetails();
            _mockContentService.GetContent(DocumentType.Glossary).ReturnsForAnyArgs(_GlossaryResultsFake.GetCommonResponseBody());
            _mockDownloadService.GetGlossaryMetaDataDownloadList().ReturnsForAnyArgs(model.MetaDataDownloadList);

            var controller = GetGlossaryController(user);

            // Act
            var result = await controller.Index().ConfigureAwait(false);

            // Assert
            var viewResult = Assert.IsAssignableFrom<ViewResult>(result);
            var viewModel = viewResult.Model as GlossaryViewModel;
            Assert.Equal(viewModel.Response.Title, model.Response.Title);
            Assert.Equal(viewModel.Response.Body, model.Response.Body);
        }

        [Fact]
        public async Task GlossaryController_GetBulkUploadTemplateFile_Should_Return_File()
        {
            // Arrange
            var user = new UserClaimsPrincipalFake().GetUserClaimsPrincipal();
            var model = _GlossaryResultsFake.GetMetaDataFile();
            _mockContentService.GetContent(DocumentType.Glossary).ReturnsForAnyArgs(_GlossaryResultsFake.GetCommonResponseBody());
            var azureFunctionHeaderDetails = new AzureFunctionHeaderDetails() { ClientId = "123456", SessionId = "654321" };
            var ms = new MemoryStream();
            _mockDownloadService.GetGlossaryMetaDataDownFileAsync("", ms, azureFunctionHeaderDetails).Returns(Task.FromResult(model));

            var controller = GetGlossaryController(user);

            // Act
            var result = await controller.GetBulkUploadTemplateFile("Test.csv").ConfigureAwait(false);

            // Assert
            Assert.Equal(result.FileDownloadName, model.FileDownloadName);
            Assert.Equal(result.ContentType, model.ContentType);
        }

        private GlossaryController GetGlossaryController(ClaimsPrincipal user)
        {
            var mockAzureAppSettings = new Mock<IOptions<AzureAppSettings>>();
            mockAzureAppSettings.Setup(x => x.Value)
                .Returns(new AzureAppSettings() { IsSessionIdStoredInCookie = false });
            return new GlossaryController(_mockContentService, _mockDownloadService)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext() { User = user }
                }
            };
        }
    }
}
