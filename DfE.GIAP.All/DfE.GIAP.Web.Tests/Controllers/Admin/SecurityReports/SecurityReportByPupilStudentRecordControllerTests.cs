using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Web.Controllers.Admin.SecurityReports;
using DfE.GIAP.Web.Tests.TestDoubles;
using DfE.GIAP.Web.ViewModels.Admin;
using DfE.GIAP.Service.Download.SecurityReport;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Linq;
using Xunit;
using DfE.GIAP.Domain.Models.Common;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using DfE.GIAP.Common.Constants.Messages.Downloads;
using System.Threading.Tasks;

namespace DfE.GIAP.Web.Tests.Controllers.Admin.SecurityReports
{
    public class SecurityReportByPupilStudentRecordControllerTests : IClassFixture<UpnResultsFake>, IClassFixture<UlnResultsFake>
    {
        private readonly DefaultHttpContext _httpContext = new DefaultHttpContext();
        private readonly Mock<IDownloadSecurityReportByUpnUlnService> _mockDownloadSecurityReportByUpnUlnService;

        public SecurityReportByPupilStudentRecordControllerTests(UpnResultsFake upnResultsFake)
        {
            _mockDownloadSecurityReportByUpnUlnService = new Mock<IDownloadSecurityReportByUpnUlnService>();
        }

        private SecurityReportByPupilStudentRecordController GetSecurityReportByPupilStudentRecordController(IOptions<AzureAppSettings> mockAzureSettings = null)
        {
            return new SecurityReportByPupilStudentRecordController(_mockDownloadSecurityReportByUpnUlnService.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = _httpContext
                }
            };
        }

        [Fact]
        public void SecurityReportByPupilStudentRecordController_LoadsView()
        {
            // Arrange
            var controller = GetSecurityReportByPupilStudentRecordController();

            // Act
            var result = controller.SecurityReportsByUpnUln();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var viewModelResult = Assert.IsType<SecurityReportsByUpnUlnViewModel>(viewResult.Model);
            Assert.NotNull(viewResult);
            Assert.NotNull(viewModelResult);
            Assert.Equal("../Admin/SecurityReports/SecurityReportsByUpnUln", viewResult.ViewName);
        }

        [Fact]
        public async Task SecurityReportByPupilStudentRecordController_DownloadSecurityReportByUpnUln_Successfully_downloads_CSV_file_when_valid_UPN_entered()
        {
            // Arrange
            var user = new UserClaimsPrincipalFake().GetUserClaimsPrincipal();
            var azureAppSettings = new AzureAppSettings() { IsSessionIdStoredInCookie = false };
            var fakeAppSettings = new Mock<IOptions<AzureAppSettings>>();
            fakeAppSettings.SetupGet(x => x.Value).Returns(azureAppSettings);
            var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user } };
            var controller = GetSecurityReportByPupilStudentRecordController(fakeAppSettings.Object);
            controller.ControllerContext = context;
            var model = new SecurityReportsByUpnUlnViewModel();
            model.UpnUln = "A01234567890A";

            var expected = new ReturnFile()
            {
                Bytes = new byte[200],
                FileName = "Test-DownloadSecurityReport-ByUPN",
                FileType = "csv"
            };

            _mockDownloadSecurityReportByUpnUlnService.Setup(x => x.GetSecurityReportByUpn(It.IsAny<string>(),
                                                         It.IsAny<AzureFunctionHeaderDetails>())).ReturnsAsync(expected);

            MockModelState(model, controller);

            // Act
            var result = await controller.DownloadSecurityReportByUpnUln(model).ConfigureAwait(false);

            // Assert
            var actionResult = Assert.IsAssignableFrom<FileContentResult>(result);
            Assert.NotNull(actionResult);
            Assert.Equal(expected.Bytes, actionResult.FileContents);
            Assert.Equal("text/" + expected.FileType, actionResult.ContentType);
            _mockDownloadSecurityReportByUpnUlnService.Verify(x => x.GetSecurityReportByUpn(It.IsAny<string>(),
                                                         It.IsAny<AzureFunctionHeaderDetails>()), Times.Once);
        }

        [Fact]
        public async Task SecurityReportByPupilStudentRecordController_DownloadSecurityReportByUpnUln_Successfully_downloads_CSV_file_when_valid_ULN_entered()
        {
            // Arrange
            var user = new UserClaimsPrincipalFake().GetUserClaimsPrincipal();
            var azureAppSettings = new AzureAppSettings() { IsSessionIdStoredInCookie = false };
            var fakeAppSettings = new Mock<IOptions<AzureAppSettings>>();
            fakeAppSettings.SetupGet(x => x.Value).Returns(azureAppSettings);
            var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user } };
            var controller = GetSecurityReportByPupilStudentRecordController(fakeAppSettings.Object);
            controller.ControllerContext = context;
            var model = new SecurityReportsByUpnUlnViewModel();
            model.UpnUln = "9999375358";

            var expected = new ReturnFile()
            {
                Bytes = new byte[200],
                FileName = "Test-DownloadSecurityReport-ByULN",
                FileType = "csv"
            };

            _mockDownloadSecurityReportByUpnUlnService.Setup(x => x.GetSecurityReportByUln(It.IsAny<string>(),
                                                         It.IsAny<AzureFunctionHeaderDetails>())).ReturnsAsync(expected);

            MockModelState(model, controller);

            // Act
            var result = await controller.DownloadSecurityReportByUpnUln(model).ConfigureAwait(false);

            // Assert
            var actionResult = Assert.IsAssignableFrom<FileContentResult>(result);
            Assert.NotNull(actionResult);
            Assert.Equal(expected.Bytes, actionResult.FileContents);
            Assert.Equal("text/" + expected.FileType, actionResult.ContentType);
            _mockDownloadSecurityReportByUpnUlnService.Verify(x => x.GetSecurityReportByUln(It.IsAny<string>(),
                                             It.IsAny<AzureFunctionHeaderDetails>()), Times.Once);
        }

        [Fact]
        public async Task SecurityReportByPupilStudentRecordController_DownloadSecurityReportByUpnUln_Adds_ModelError_If_No_UPN_Or_ULN_Entered()
        {
            // Arrange
            var user = new UserClaimsPrincipalFake().GetUserClaimsPrincipal();
            var azureAppSettings = new AzureAppSettings() { IsSessionIdStoredInCookie = false };
            var fakeAppSettings = new Mock<IOptions<AzureAppSettings>>();
            fakeAppSettings.SetupGet(x => x.Value).Returns(azureAppSettings);
            var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user } };
            var controller = GetSecurityReportByPupilStudentRecordController(fakeAppSettings.Object);
            controller.ControllerContext = context;
            var model = new SecurityReportsByUpnUlnViewModel();
            model.UpnUln = null;

            MockModelState(model, controller);

            // Act
            var result = await controller.DownloadSecurityReportByUpnUln(model).ConfigureAwait(false);

            // Assert
            var viewResult = Assert.IsAssignableFrom<ViewResult>(result);
            Assert.NotNull(viewResult);
            Assert.Equal("../Admin/SecurityReports/SecurityReportsByUpnUln", viewResult.ViewName);
            Assert.True(controller.ViewData.ModelState["EmptyUpnOrUln"].Errors.Count == 1);
        }

        [Fact]
        public async Task SecurityReportByPupilStudentRecordController_DownloadSecurityReportByUpnUln_Adds_ModelError_If_No_Results_Found()
        {
            // Arrange
            var user = new UserClaimsPrincipalFake().GetUserClaimsPrincipal();
            var azureAppSettings = new AzureAppSettings() { IsSessionIdStoredInCookie = false };
            var fakeAppSettings = new Mock<IOptions<AzureAppSettings>>();
            fakeAppSettings.SetupGet(x => x.Value).Returns(azureAppSettings);
            var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user } };
            var controller = GetSecurityReportByPupilStudentRecordController(fakeAppSettings.Object);
            controller.ControllerContext = context;

            var model = new SecurityReportsByUpnUlnViewModel();
            model.UpnUln = "xxxxxx";

            // Act
            var result = await controller.DownloadSecurityReportByUpnUln(model).ConfigureAwait(false);

            // Assert
            var viewResult = Assert.IsAssignableFrom<ViewResult>(result);
            var viewModelResult = Assert.IsType<SecurityReportsByUpnUlnViewModel>(viewResult.Model);
            Assert.NotNull(viewResult);
            Assert.Equal("../Admin/SecurityReports/SecurityReportsByUpnUln", viewResult.ViewName);
            Assert.True(viewModelResult.ErrorDetails.Equals(DownloadErrorMessages.NoDataForDownload));
        }

        /*https://bytelanguage.net/2020/07/31/writing-unit-test-for-model-validation/*/
        private void MockModelState<TModel, TController>(TModel model, TController controller) where TController : ControllerBase
        {
            var validationContext = new ValidationContext(model, null, null);
            var validationResults = new List<ValidationResult>();
            Validator.TryValidateObject(model, validationContext, validationResults, true);
            foreach (var validationResult in validationResults)
            {
                controller.ModelState.AddModelError(validationResult.MemberNames.First(), validationResult.ErrorMessage);
            }
        }
    }
}
