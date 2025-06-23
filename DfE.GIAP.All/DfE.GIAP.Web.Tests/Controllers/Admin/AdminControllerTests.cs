using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Service.Download.SecurityReport;
using DfE.GIAP.Service.Security;
using DfE.GIAP.Web.Controllers.Admin;
using DfE.GIAP.Web.Tests.FakeData;
using DfE.GIAP.Web.ViewModels.Admin.SecurityReports;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using NSubstitute;
using DfE.GIAP.Common.Enums;
using Xunit;
using System.Threading.Tasks;
using DfE.GIAP.Web.ViewModels.Admin;
using System.Collections.Generic;
using DfE.GIAP.Domain.Models.SecurityReports;
using DfE.GIAP.Web.Providers.Session;

namespace DfE.GIAP.Web.Tests.Controllers.Admin
{
    [Trait("Category", "Admin Controller Unit Tests")]
    public class AdminControllerTests : IClassFixture<UserClaimsPrincipalFake>
    {
        private readonly UserClaimsPrincipalFake _userClaimsPrincipalFake;
        private readonly ISecurityService _securityService = Substitute.For<ISecurityService>();
        private readonly ISessionProvider _sessionProvider = Substitute.For<ISessionProvider>();

        private readonly IOptions<AzureAppSettings>
            _mockAzureAppSettings = Substitute.For<IOptions<AzureAppSettings>>();

        private readonly IDownloadSecurityReportByUpnUlnService _downloadSecurityReportByUpnService =
            Substitute.For<IDownloadSecurityReportByUpnUlnService>();

        private readonly IDownloadSecurityReportLoginDetailsService _downloadSecurityReportLoginDetailsService =
            Substitute.For<IDownloadSecurityReportLoginDetailsService>();

        private readonly IDownloadSecurityReportDetailedSearchesService _downloadSecurityReportDetailedSearchesService =
            Substitute.For<IDownloadSecurityReportDetailedSearchesService>();

        public AdminControllerTests(UserClaimsPrincipalFake userClaimsPrincipalFake)
        {
            _userClaimsPrincipalFake = userClaimsPrincipalFake;
        }

        private AdminController GetAdminController()
        {
            return new AdminController(_securityService,
                _sessionProvider,
                _downloadSecurityReportByUpnService,
                _downloadSecurityReportLoginDetailsService,
                _downloadSecurityReportDetailedSearchesService);
        }

        [Fact]
        public void AdminController_AdminViewLoadsSuccessfully()
        {
            // Arrange
            var user = new UserClaimsPrincipalFake().GetAdminUserClaimsPrincipal();
            var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user } };
            var controller = GetAdminController();
            controller.ControllerContext = context;

            // Act
            var result = controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
            Assert.Equal("../Admin/Index", viewResult.ViewName);
        }

        [Fact]
        public void AdminController_DashboardOptions_Returns_ManageDocuments_Redirect_To_Action()
        {
            // Arrange
            var user = new UserClaimsPrincipalFake().GetUserClaimsPrincipal();
            var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user } };
            var controller = GetAdminController();
            controller.ControllerContext = context;

            var model = new AdminViewModel();
            model.SelectedAdminOption = "ManageDocuments";

            // Act
            var result = controller.AdminOptions(model);

            // Assert
            var viewResult = Assert.IsAssignableFrom<RedirectToActionResult>(result);
            Assert.True(viewResult.ControllerName.Equals("ManageDocuments"));
            Assert.True(viewResult.ActionName.Equals("ManageDocuments"));
        }

        [Fact]
        public void AdminController_DashboardOptions_Returns_SecurityReportsByUpnUln_Redirect_To_Action()
        {
            // Arrange
            var user = new UserClaimsPrincipalFake().GetUserClaimsPrincipal();
            var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user } };
            var controller = GetAdminController();
            controller.ControllerContext = context;

            var model = new AdminViewModel();
            model.SelectedAdminOption = "DownloadSecurityReportsByPupilOrStudent";

            // Act
            var result = controller.AdminOptions(model);

            // Assert
            var viewResult = Assert.IsAssignableFrom<RedirectToActionResult>(result);
            Assert.True(viewResult.ControllerName.Equals("SecurityReportByPupilStudentRecord"));
            Assert.True(viewResult.ActionName.Equals("SecurityReportsByUpnUln"));
        }

        [Fact]
        public void AdminController_DashboardOptions_Returns_SecurityReportsForYourOrganisation_Redirect_To_Action()
        {
            // Arrange
            var user = new UserClaimsPrincipalFake().GetUserClaimsPrincipal();
            var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user } };
            var controller = GetAdminController();
            controller.ControllerContext = context;

            var model = new AdminViewModel();
            model.SelectedAdminOption = "DownloadSecurityReportsByOrganisation";

            // Act
            var result = controller.AdminOptions(model);

            // Assert
            var viewResult = Assert.IsAssignableFrom<RedirectToActionResult>(result);
            Assert.True(viewResult.ControllerName.Equals("Admin"));
            Assert.True(viewResult.ActionName.Equals("SecurityReportsForYourOrganisation"));
        }

        [Fact]
        public void AdminController_DashboardOptions_Returns_DownloadSecurityReportsBySchool_Admin_Redirect_To_Action()
        {
            // Arrange
            var user = new UserClaimsPrincipalFake().GetAdminUserClaimsPrincipal();
            var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user } };
            var controller = GetAdminController();
            controller.ControllerContext = context;

            var model = new AdminViewModel();
            model.SelectedAdminOption = "DownloadSecurityReportsBySchool";

            // Act
            var result = controller.AdminOptions(model);

            // Assert
            var viewResult = Assert.IsAssignableFrom<RedirectToActionResult>(result);
            Assert.True(viewResult.ControllerName.Equals("Admin"));
            Assert.True(viewResult.ActionName.Equals("SchoolCollegeDownloadOptions"));
        }

        [Fact]
        public void AdminController_DashboardOptions_Returns_DownloadSecurityReportsBySchool_NonAdmin_Redirect_To_Action()
        {
            // Arrange
            var user = new UserClaimsPrincipalFake().GetUserClaimsPrincipal();
            var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user } };
            var controller = GetAdminController();
            controller.ControllerContext = context;

            var model = new AdminViewModel();
            model.SelectedAdminOption = "DownloadSecurityReportsBySchool";

            // Act
            var result = controller.AdminOptions(model);

            // Assert
            var viewResult = Assert.IsAssignableFrom<RedirectToActionResult>(result);
            Assert.True(viewResult.ControllerName.Equals("Admin"));
            Assert.True(viewResult.ActionName.Equals("SecurityReportsBySchool"));
        }

        [Fact]
        public void AdminController_DashboardOptions_Returns_ValidationMessage_If_No_Selection_Made()
        {
            // Arrange
            var user = new UserClaimsPrincipalFake().GetAdminUserClaimsPrincipal();
            var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user } };
            var controller = GetAdminController();
            controller.ControllerContext = context;

            var model = new AdminViewModel();
            model.SelectedAdminOption = null;

            // Act
            var result = controller.AdminOptions(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var viewModel = viewResult.Model as AdminViewModel;
            Assert.NotNull(viewResult);
            Assert.NotNull(viewModel);
            Assert.Equal("../Admin/Index", viewResult.ViewName);
            Assert.True(controller.ViewData.ModelState["NoAdminSelection"].Errors.Count == 1);
        }

        [Fact]
        public void AdminController_SchoolCollegeDownloadOptionsAdminGet_Renders_Correct_View()
        {
            // Arrange
            var user = new UserClaimsPrincipalFake().GetAdminUserClaimsPrincipal();
            var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user } };
            var controller = GetAdminController();
            controller.ControllerContext = context;

            // Act
            var result = controller.SchoolCollegeDownloadOptions();

            // Assert
            var viewResult = Assert.IsAssignableFrom<ViewResult>(result);
            Assert.NotNull(viewResult);
            Assert.Equal("../Admin/SecurityReports/SchoolCollegeDownloadOptions", viewResult.ViewName);
        }

        [Fact]
        public void AdminController_SchoolCollegeDownloadOptions_SecurityReportsBySchool_Redirect_To_Action()
        {
            // Arrange
            var user = new UserClaimsPrincipalFake().GetAdminUserClaimsPrincipal();
            var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user } };
            var controller = GetAdminController();
            controller.ControllerContext = context;

            var model = new AdminViewModel();
            model.SelectedOrganisationOption = "AcademyTrust";

            // Act
            var result = controller.SchoolCollegeDownloadOptions(model);

            // Assert
            var viewResult = Assert.IsAssignableFrom<RedirectToActionResult>(result);
            Assert.True(viewResult.ControllerName.Equals("Admin"));
            Assert.True(viewResult.ActionName.Equals("SecurityReportsBySchool"));
        }

        [Fact]
        public void AdminController_SchoolCollegeDownloadOptions_Returns_Validation_Message_If_No_Selection_Made()
        {
            // Arrange
            var user = new UserClaimsPrincipalFake().GetAdminUserClaimsPrincipal();
            var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user } };
            var controller = GetAdminController();
            controller.ControllerContext = context;

            var model = new AdminViewModel();
            model.SelectedOrganisationOption = null;

            // Act
            var result = controller.SchoolCollegeDownloadOptions(model);

            // Assert
            var viewResult = Assert.IsAssignableFrom<ViewResult>(result);
            Assert.NotNull(viewResult);
            Assert.Equal("../Admin/SecurityReports/SchoolCollegeDownloadOptions", viewResult.ViewName);
            Assert.True(
                controller.ViewData.ModelState["NoOrganisationSelection"].Errors.Count == 1);
        }

        [Fact]
        public async Task AdminController_SecurityReportsBySchoolGet_Renders_Correct_View()
        {
            // Arrange
            var user = new UserClaimsPrincipalFake().GetAdminUserClaimsPrincipal();
            var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user } };
            var controller = GetAdminController();
            controller.ControllerContext = context;

            // Act
            var result = await controller.SecurityReportsBySchool().ConfigureAwait(false);

            // Assert
            var viewResult = Assert.IsAssignableFrom<ViewResult>(result);
            Assert.NotNull(viewResult);
            Assert.Equal("../Admin/SecurityReports/SecurityReportsBySchool", viewResult.ViewName);
        }

        [Fact]
        public async Task AdminController_SecurityReportsBySchoolEstablishmentSelectionGet_Renders_Correct_View()
        {
            // Arrange
            var user = new UserClaimsPrincipalFake().GetUserClaimsPrincipal();
            var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user } };
            var controller = GetAdminController();
            controller.ControllerContext = context;

            // Act
            var result = await controller.SecurityReportsBySchoolEstablishmentSelection().ConfigureAwait(false);

            // Assert
            var viewResult = Assert.IsAssignableFrom<ViewResult>(result);
            Assert.NotNull(viewResult);
            Assert.Equal("../Admin/SecurityReports/SecurityReportsBySchoolEstablishmentSelection", viewResult.ViewName);
        }

        [Fact]
        public void AdminController_SecurityReportsBySchoolConfirmationGet_Renders_Correct_View()
        {
            // Arrange
            var user = new UserClaimsPrincipalFake().GetUserClaimsPrincipal();
            var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user } };
            var controller = GetAdminController();
            controller.ControllerContext = context;

            // Act
            var result = controller.SecurityReportsBySchoolConfirmation();

            // Assert
            var viewResult = Assert.IsAssignableFrom<ViewResult>(result);
            Assert.NotNull(viewResult);
            Assert.Equal("../Admin/SecurityReports/SecurityReportsBySchoolConfirmation", viewResult.ViewName);
        }

        [Fact]
        public void AdminController_SecurityReportsForYourOrganisationGet_Renders_Correct_View()
        {
            // Arrange
            var user = new UserClaimsPrincipalFake().GetUserClaimsPrincipal();
            var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user } };
            var controller = GetAdminController();
            controller.ControllerContext = context;

            // Act
            var result = controller.SecurityReportsForYourOrganisation();

            // Assert
            var viewResult = Assert.IsAssignableFrom<ViewResult>(result);
            Assert.NotNull(viewResult);
            Assert.Equal("../Admin/SecurityReports/SecurityReportsForYourOrganisation", viewResult.ViewName);
        }

        [Fact]
        public async Task AdminController_SecurityReportsBySchoolPost_Adds_ModelError_If_Neither_LA_or_AT_Selected()
        {
            // Arrange
            var user = new UserClaimsPrincipalFake().GetAdminUserClaimsPrincipal();
            var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user } };
            var controller = GetAdminController();
            controller.ControllerContext = context;

            var model = new SecurityReportsBySchoolViewModel();
            model.SelectedOrganisationCode = null;

            // Act
            var result = await controller.SecurityReportsBySchool(model).ConfigureAwait(false);

            // Assert
            var viewResult = Assert.IsAssignableFrom<ViewResult>(result);
            Assert.NotNull(viewResult);
            Assert.Equal("../Admin/SecurityReports/SecurityReportsBySchool", viewResult.ViewName);
            Assert.True(
                controller.ViewData.ModelState["NoOrganisationSelected"].Errors.Count == 1);
        }

        [Fact]
        public async Task AdminController_DownloadSecurityReportsBySchoolPost_Adds_ModelError_If_Neither_LA_or_AT_Selected_And_No_Establishment()
        {
            // Arrange
            var user = new UserClaimsPrincipalFake().GetAdminUserClaimsPrincipal();
            var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user } };
            var controller = GetAdminController();
            controller.ControllerContext = context;

            var model = new SecurityReportsBySchoolViewModel();
            model.SelectedEstablishmentCode = null;
            model.SelectedOrganisationCode = null;

            // Act
            var result = await controller.DownloadSecurityReportsBySchool(model).ConfigureAwait(false);

            // Assert
            var viewResult = Assert.IsAssignableFrom<ViewResult>(result);
            Assert.NotNull(viewResult);
            Assert.Equal("../Admin/SecurityReports/SecurityReportsBySchoolEstablishmentSelection", viewResult.ViewName);
            Assert.True(
                controller.ViewData.ModelState["NoOrganisationSelected"].Errors.Count == 1);
        }

        [Fact]
        public async Task AdminController_DownloadSecurityReportsBySchoolPost_Adds_ModelError_If_Both_LA_and_AT_Selected_And_No_Establishment()
        {
            // Arrange
            var user = new UserClaimsPrincipalFake().GetAdminUserClaimsPrincipal();
            var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user } };
            var controller = GetAdminController();
            controller.ControllerContext = context;

            var model = new SecurityReportsBySchoolViewModel();
            model.SelectedEstablishmentCode = null;
            model.SelectedOrganisationCode = "Test LA";

            // Act
            var result = await controller.DownloadSecurityReportsBySchool(model).ConfigureAwait(false);

            // Assert
            var viewResult = Assert.IsAssignableFrom<ViewResult>(result);
            Assert.NotNull(viewResult);
            Assert.Equal("../Admin/SecurityReports/SecurityReportsBySchoolEstablishmentSelection", viewResult.ViewName);
            Assert.True(!controller.ViewData.ModelState.IsValid);
            Assert.True(
                controller.ViewData.ModelState["NoEstablishmentSelected"].Errors.Count == 1);
            Assert.True(
                controller.ViewData.ModelState["NoEstablishmentSelected"].Errors[0].ErrorMessage ==
                SecurityReportsConstants.NoEstablishmentSelected);
        }

        [Fact]
        public async Task AdminController_DownloadSecurityReportsBySchoolPost_Sets_Correct_Model_Properties_If_No_Establishment()
        {
            // Arrange
            var user = new UserClaimsPrincipalFake().GetAdminUserClaimsPrincipal();
            var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user } };
            var controller = GetAdminController();
            controller.ControllerContext = context;

            var model = new SecurityReportsBySchoolViewModel();
            model.SelectedEstablishmentCode = null;
            model.SelectedOrganisationCode = "Test LA";
            model.SelectedReportType = "Test report type";

            // Act
            var result = await controller.DownloadSecurityReportsBySchool(model).ConfigureAwait(false);

            // Assert
            var viewResult = Assert.IsAssignableFrom<ViewResult>(result);
            Assert.NotNull(viewResult);
            Assert.Equal("../Admin/SecurityReports/SecurityReportsBySchoolEstablishmentSelection", viewResult.ViewName);
            Assert.Equal(model.SelectedReportType, controller.ViewBag.SelectedReportType);
            Assert.Equal(model.SelectedOrganisationCode, controller.ViewBag.SelectedOrganisationCode);
            Assert.True(controller.ViewData.ModelState["NoEstablishmentSelected"].Errors.Count == 1);
            Assert.True(controller.ViewData.ModelState["NoEstablishmentSelected"].Errors[0].ErrorMessage ==
                        SecurityReportsConstants.NoEstablishmentSelected);
        }

        [Fact]
        public async Task AdminController_DownloadSecurityReportsBySchoolPost_Returns_Correct_Data()
        {
            // Arrange
            var user = _userClaimsPrincipalFake.GetUserClaimsPrincipal();
            var mockSession = new Mock<ISession>();
            mockSession.Setup(x => x.Id).Returns("12345");
            var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user, Session = mockSession.Object } };

            var mockAzureAppSettings = new Mock<IOptions<AzureAppSettings>>();
            mockAzureAppSettings.Setup(x => x.Value)
                .Returns(new AzureAppSettings() { IsSessionIdStoredInCookie = false });

            var expected = new ReturnFile()
            {
                Bytes = new byte[200],
                FileName = "Test-DownloadSecurityReport-ByURN",
                FileType = "csv"
            };
            _downloadSecurityReportLoginDetailsService.GetSecurityReportLoginDetails(Arg.Any<string>(),
                    Arg.Any<SecurityReportSearchType>(), Arg.Any<AzureFunctionHeaderDetails>())
                .Returns(expected);
            var controller = new AdminController(
                _securityService,
                _sessionProvider,
                _downloadSecurityReportByUpnService,
                _downloadSecurityReportLoginDetailsService,
                _downloadSecurityReportDetailedSearchesService);
            controller.ControllerContext = context;

            var model = new SecurityReportsBySchoolViewModel();
            model.SelectedEstablishmentCode = "123456";
            model.SelectedOrganisationCode = "Test LA";
            model.SelectedReportType = "LoginDetails";

            // Act
            var result = await controller.DownloadSecurityReportsBySchool(model).ConfigureAwait(false) as ViewResult;

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var viewModel = viewResult.Model as SecurityReportsBySchoolViewModel;
            Assert.NotNull(viewResult);
            Assert.NotNull(viewModel);
            Assert.True(viewModel.ProcessDownload);
            Assert.Equal("../Admin/SecurityReports/SecurityReportsBySchoolEstablishmentSelection", viewResult.ViewName);
        }

        [Fact]
        public async Task AdminController_DownloadLoginDetailsSecurityReportsBySchool_By_UniqueReferenceNumber_Post_Returns_Correct_Data()
        {
            // Arrange
            var user = _userClaimsPrincipalFake.GetUserClaimsPrincipal();
            var mockSession = new Mock<ISession>();
            mockSession.Setup(x => x.Id).Returns("12345");
            var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user, Session = mockSession.Object } };

            var mockAzureAppSettings = new Mock<IOptions<AzureAppSettings>>();
            mockAzureAppSettings.Setup(x => x.Value)
                .Returns(new AzureAppSettings() { IsSessionIdStoredInCookie = false });

            var expected = new ReturnFile()
            {
                Bytes = new byte[200],
                FileName = "Test-DownloadSecurityReport-ByURN",
                FileType = "csv"
            };
            _downloadSecurityReportLoginDetailsService.GetSecurityReportLoginDetails(Arg.Any<string>(),
                    Arg.Any<SecurityReportSearchType>(), Arg.Any<AzureFunctionHeaderDetails>())
                .Returns(expected);
            var controller = new AdminController(
                _securityService,
                _sessionProvider,
                _downloadSecurityReportByUpnService,
                _downloadSecurityReportLoginDetailsService,
                _downloadSecurityReportDetailedSearchesService);
            controller.ControllerContext = context;

            var model = new SecurityReportsBySchoolViewModel();
            model.SelectedEstablishmentCode = "123456";
            model.SelectedOrganisationCode = null;
            model.SelectedReportType = "LoginDetails";

            // Act
            var result = await controller.DownloadSecurityReportsBySchool(model).ConfigureAwait(false) as ViewResult;

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var viewModel = viewResult.Model as SecurityReportsBySchoolViewModel;
            Assert.NotNull(viewResult);
            Assert.NotNull(viewModel);
            Assert.True(viewModel.ProcessDownload);
            Assert.Equal("../Admin/SecurityReports/SecurityReportsBySchoolEstablishmentSelection", viewResult.ViewName);
        }

        [Fact]
        public async Task AdminController_DownloadSecurityReportsBySchool_By_SATApprover_Post_Returns_Correct_Data()
        {
            // Arrange
            var user = _userClaimsPrincipalFake.GetSATApproverClaimsPrincipal();
            var mockSession = new Mock<ISession>();
            mockSession.Setup(x => x.Id).Returns("12345");
            var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user, Session = mockSession.Object } };

            var mockAzureAppSettings = new Mock<IOptions<AzureAppSettings>>();
            mockAzureAppSettings.Setup(x => x.Value)
                .Returns(new AzureAppSettings() { IsSessionIdStoredInCookie = false });

            var expected = new ReturnFile()
            {
                Bytes = new byte[200],
                FileName = "Test-DownloadSecurityReport-ByUPN",
                FileType = "csv"
            };
            _downloadSecurityReportLoginDetailsService.GetSecurityReportLoginDetails(Arg.Any<string>(),
                    Arg.Any<SecurityReportSearchType>(), Arg.Any<AzureFunctionHeaderDetails>())
                .Returns(expected);

            var controller = new AdminController(
                _securityService,
                _sessionProvider,
                _downloadSecurityReportByUpnService,
                _downloadSecurityReportLoginDetailsService,
                _downloadSecurityReportDetailedSearchesService);

            controller.ControllerContext = context;

            var model = new SecurityReportsBySchoolViewModel();
            model.SelectedOrganisationCode = "12345";
            model.SelectedEstablishmentCode = "6789";
            model.SelectedReportType = "LoginDetails";

            // Act
            var result = await controller.DownloadSecurityReportsBySchool(model).ConfigureAwait(false) as ViewResult;

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var viewModel = viewResult.Model as SecurityReportsBySchoolViewModel;
            Assert.NotNull(viewResult);
            Assert.NotNull(viewModel);
            Assert.True(viewModel.ProcessDownload);
            Assert.Equal("../Admin/SecurityReports/SecurityReportsBySchoolEstablishmentSelection", viewResult.ViewName);
        }


        [Fact]
        public async Task AdminController_DownloadSecurityReportsBySchool_DetailedSearches_By_UniqueReferenceNumber_Post_Returns_Correct_Data()
        {
            // Arrange
            var user = _userClaimsPrincipalFake.GetUserClaimsPrincipal();
            var mockSession = new Mock<ISession>();
            mockSession.Setup(x => x.Id).Returns("12345");
            var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user, Session = mockSession.Object } };

            var mockAzureAppSettings = new Mock<IOptions<AzureAppSettings>>();
            mockAzureAppSettings.Setup(x => x.Value)
                .Returns(new AzureAppSettings() { IsSessionIdStoredInCookie = false });

            var expected = new ReturnFile()
            {
                Bytes = new byte[200],
                FileName = "SecurityReport_DetailedSearches",
                FileType = "csv"
            };
            _downloadSecurityReportDetailedSearchesService.GetSecurityReportDetailedSearches(Arg.Any<string>(),
                    Arg.Any<SecurityReportSearchType>(), Arg.Any<AzureFunctionHeaderDetails>(), Arg.Any<bool>())
                .Returns(expected);
            var controller = new AdminController(
                 _securityService,
                 _sessionProvider,
                 _downloadSecurityReportByUpnService,
                 _downloadSecurityReportLoginDetailsService,
                 _downloadSecurityReportDetailedSearchesService);
            controller.ControllerContext = context;

            var model = new SecurityReportsBySchoolViewModel();
            model.SelectedEstablishmentCode = "123456";
            model.SelectedOrganisationCode = null;
            model.SelectedReportType = "DetailedSearches";

            // Act
            var result = await controller.DownloadSecurityReportsBySchool(model).ConfigureAwait(false) as ViewResult;

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var viewModel = viewResult.Model as SecurityReportsBySchoolViewModel;
            Assert.NotNull(viewResult);
            Assert.NotNull(viewModel);
            Assert.True(viewModel.ProcessDownload);
            Assert.Equal("../Admin/SecurityReports/SecurityReportsBySchoolEstablishmentSelection", viewResult.ViewName);
        }

        [Fact]
        public async Task AdminController_DownloadSecurityReportsBySchool_DetailedSearches_By_SATApprover_Post_Returns_Correct_Data()
        {
            // Arrange
            var user = _userClaimsPrincipalFake.GetSATApproverClaimsPrincipal();
            var mockSession = new Mock<ISession>();
            mockSession.Setup(x => x.Id).Returns("12345");
            var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user, Session = mockSession.Object } };

            var mockAzureAppSettings = new Mock<IOptions<AzureAppSettings>>();
            mockAzureAppSettings.Setup(x => x.Value)
                .Returns(new AzureAppSettings() { IsSessionIdStoredInCookie = false });

            var expected = new ReturnFile()
            {
                Bytes = new byte[200],
                FileName = "SecurityReport_DetailedSearches",
                FileType = "csv"
            };
            _downloadSecurityReportDetailedSearchesService.GetSecurityReportDetailedSearches(Arg.Any<string>(),
                    Arg.Any<SecurityReportSearchType>(), Arg.Any<AzureFunctionHeaderDetails>(), Arg.Any<bool>())
                .Returns(expected);

            var controller = new AdminController(
                 _securityService,
                 _sessionProvider,
                 _downloadSecurityReportByUpnService,
                 _downloadSecurityReportLoginDetailsService,
                 _downloadSecurityReportDetailedSearchesService);

            controller.ControllerContext = context;

            var model = new SecurityReportsBySchoolViewModel();
            model.SelectedOrganisationCode = "12345";
            model.SelectedEstablishmentCode = "6789";
            model.SelectedReportType = "DetailedSearches";

            // Act
            var result = await controller.DownloadSecurityReportsBySchool(model).ConfigureAwait(false) as ViewResult;

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var viewModel = viewResult.Model as SecurityReportsBySchoolViewModel;
            Assert.NotNull(viewResult);
            Assert.NotNull(viewModel);
            Assert.True(viewModel.ProcessDownload);
            Assert.Equal("../Admin/SecurityReports/SecurityReportsBySchoolEstablishmentSelection", viewResult.ViewName);
        }

        [Fact]
        public async Task AdminController_GetSecurityReport_Returns_Correct_Data()
        {
            // Arrange
            var user = _userClaimsPrincipalFake.GetAdminUserClaimsPrincipal();
            var mockSession = new Mock<ISession>();
            mockSession.Setup(x => x.Id).Returns("12345");
            var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user, Session = mockSession.Object } };

            var mockAzureAppSettings = new Mock<IOptions<AzureAppSettings>>();
            mockAzureAppSettings.Setup(x => x.Value)
                .Returns(new AzureAppSettings() { IsSessionIdStoredInCookie = false });

            var expected = new ReturnFile()
            {
                Bytes = new byte[200],
                FileName = "SecurityReport_DetailedSearches",
                FileType = "csv"
            };

            _downloadSecurityReportDetailedSearchesService.GetSecurityReportDetailedSearches(Arg.Any<string>(),
                    Arg.Any<SecurityReportSearchType>(), Arg.Any<AzureFunctionHeaderDetails>(), Arg.Any<bool>())
                .Returns(expected);

            var controller = new AdminController(
                 _securityService,
                 _sessionProvider,
                 _downloadSecurityReportByUpnService,
                 _downloadSecurityReportLoginDetailsService,
                 _downloadSecurityReportDetailedSearchesService);

            controller.ControllerContext = context;

            // Act
            var result = await controller.GetSecurityReport("detailedsearches", "001", "LocalAuthority").ConfigureAwait(false) as FileContentResult;

            // Assert
            Assert.IsType<FileContentResult>(result);
            Assert.Equal(expected.Bytes.Length, result.FileContents.Length);
        }

        [Fact]
        public async Task AdminController_GetSecurityReport_Returns_Correct_Data_For_SAT_Approver()
        {
            // Arrange
            var user = _userClaimsPrincipalFake.GetSATApproverClaimsPrincipal();
            var mockSession = new Mock<ISession>();
            mockSession.Setup(x => x.Id).Returns("12345");
            var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user, Session = mockSession.Object } };

            var mockAzureAppSettings = new Mock<IOptions<AzureAppSettings>>();
            mockAzureAppSettings.Setup(x => x.Value)
                .Returns(new AzureAppSettings() { IsSessionIdStoredInCookie = false });

            var expectedEstablishments = new List<Establishment>()
            {
                new Establishment()
                {
                    Name = "Test_SAT",
                    URN = "013"
                }
            };

            var expected = new ReturnFile()
            {
                Bytes = new byte[200],
                FileName = "SecurityReport_DetailedSearches",
                FileType = "csv"
            };

            _securityService.GetEstablishmentsByAcademyTrustCode(Arg.Any<List<string>>(), Arg.Any<string>())
                .Returns(expectedEstablishments);

            _downloadSecurityReportDetailedSearchesService.GetSecurityReportDetailedSearches(Arg.Any<string>(),
                    Arg.Any<SecurityReportSearchType>(), Arg.Any<AzureFunctionHeaderDetails>(), Arg.Any<bool>())
                .Returns(expected);

            var controller = new AdminController(
                _securityService,
                _sessionProvider,
                _downloadSecurityReportByUpnService,
                _downloadSecurityReportLoginDetailsService,
                _downloadSecurityReportDetailedSearchesService);

            controller.ControllerContext = context;

            // Act
            var result = await controller.GetSecurityReport("detailedsearches", "013", "LocalAuthority").ConfigureAwait(false) as FileContentResult;

            // Assert
            Assert.IsType<FileContentResult>(result);
            Assert.Equal(expected.Bytes.Length, result.FileContents.Length);
        }

        [Fact]
        public async Task AdminController_GetSecurityReport_Returns_Correct_Data_For_LA_Approver()
        {
            // Arrange
            var user = _userClaimsPrincipalFake.GetLAApproverClaimsPrincipal();
            var mockSession = new Mock<ISession>();
            mockSession.Setup(x => x.Id).Returns("12345");
            var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user, Session = mockSession.Object } };

            var mockAzureAppSettings = new Mock<IOptions<AzureAppSettings>>();
            mockAzureAppSettings.Setup(x => x.Value)
                .Returns(new AzureAppSettings() { IsSessionIdStoredInCookie = false });

            var expectedEstablishments = new List<Establishment>()
            {
                new Establishment()
                {
                    Name = "Test_LA",
                    URN = "002"
                }
            };

            var expected = new ReturnFile()
            {
                Bytes = new byte[200],
                FileName = "SecurityReport_DetailedSearches",
                FileType = "csv"
            };

            _securityService.GetEstablishmentsByOrganisationCode(Arg.Any<string>(), Arg.Any<string>())
                .Returns(expectedEstablishments);

            _downloadSecurityReportDetailedSearchesService.GetSecurityReportDetailedSearches(Arg.Any<string>(),
                    Arg.Any<SecurityReportSearchType>(), Arg.Any<AzureFunctionHeaderDetails>(), Arg.Any<bool>())
                .Returns(expected);

            var controller = new AdminController(
                 _securityService,
                 _sessionProvider,
                 _downloadSecurityReportByUpnService,
                 _downloadSecurityReportLoginDetailsService,
                 _downloadSecurityReportDetailedSearchesService);

            controller.ControllerContext = context;

            // Act
            var result = await controller.GetSecurityReport("detailedsearches", "002", "LocalAuthority").ConfigureAwait(false) as FileContentResult;

            // Assert
            Assert.IsType<FileContentResult>(result);
            Assert.Equal(expected.Bytes.Length, result.FileContents.Length);
        }

        [Fact]
        public async Task AdminController_GetSecurityReport_Returns_Correct_Data_For_FE_Approver()
        {
            // Arrange
            var user = _userClaimsPrincipalFake.GetFEApproverClaimsPrincipal();
            var mockSession = new Mock<ISession>();
            mockSession.Setup(x => x.Id).Returns("12345");
            var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user, Session = mockSession.Object } };

            var mockAzureAppSettings = new Mock<IOptions<AzureAppSettings>>();
            mockAzureAppSettings.Setup(x => x.Value)
                .Returns(new AzureAppSettings() { IsSessionIdStoredInCookie = false });

            var expectedEstablishments = new List<Establishment>()
            {
                new Establishment()
                {
                    Name = "Test_FE",
                    URN = "001"
                }
            };

            var expected = new ReturnFile()
            {
                Bytes = new byte[200],
                FileName = "SecurityReport_DetailedSearches",
                FileType = "csv"
            };

            _securityService.GetEstablishmentsByOrganisationCode(Arg.Any<string>(), Arg.Any<string>())
                .Returns(expectedEstablishments);

            _downloadSecurityReportDetailedSearchesService.GetSecurityReportDetailedSearches(Arg.Any<string>(),
                    Arg.Any<SecurityReportSearchType>(), Arg.Any<AzureFunctionHeaderDetails>(), Arg.Any<bool>())
                .Returns(expected);

            var controller = new AdminController(
                _securityService,
                _sessionProvider,
                _downloadSecurityReportByUpnService,
                _downloadSecurityReportLoginDetailsService,
                _downloadSecurityReportDetailedSearchesService);

            controller.ControllerContext = context;

            // Act
            var result = await controller.GetSecurityReport("detailedsearches", "001", "LocalAuthority").ConfigureAwait(false) as FileContentResult;

            // Assert
            Assert.IsType<FileContentResult>(result);
            Assert.Equal(expected.Bytes.Length, result.FileContents.Length);
        }

        [Fact]
        public async Task AdminController_GetSecurityReport_Returns_Correct_Data_For_Organisation_User()
        {
            // Arrange
            var user = _userClaimsPrincipalFake.GetUserClaimsPrincipal();
            var mockSession = new Mock<ISession>();
            mockSession.Setup(x => x.Id).Returns("12345");
            var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user, Session = mockSession.Object } };

            var mockAzureAppSettings = new Mock<IOptions<AzureAppSettings>>();
            mockAzureAppSettings.Setup(x => x.Value)
                .Returns(new AzureAppSettings() { IsSessionIdStoredInCookie = false });

            var expectedEstablishments = new List<Establishment>()
            {
                new Establishment()
                {
                    Name = "Test_Org",
                    URN = "121"
                }
            };

            var expected = new ReturnFile()
            {
                Bytes = new byte[200],
                FileName = "SecurityReport_DetailedSearches",
                FileType = "csv"
            };

            _securityService.GetEstablishmentsByOrganisationCode(Arg.Any<string>(), Arg.Any<string>())
                .Returns(expectedEstablishments);

            _downloadSecurityReportDetailedSearchesService.GetSecurityReportDetailedSearches(Arg.Any<string>(),
                    Arg.Any<SecurityReportSearchType>(), Arg.Any<AzureFunctionHeaderDetails>(), Arg.Any<bool>())
                .Returns(expected);

            var controller = new AdminController(
                _securityService,
                _sessionProvider,
                _downloadSecurityReportByUpnService,
                _downloadSecurityReportLoginDetailsService,
                _downloadSecurityReportDetailedSearchesService);

            controller.ControllerContext = context;

            // Act
            var result = await controller.GetSecurityReport("detailedsearches", "121", "LocalAuthority").ConfigureAwait(false) as FileContentResult;

            // Assert
            Assert.IsType<FileContentResult>(result);
            Assert.Equal(expected.Bytes.Length, result.FileContents.Length);
        }

        [Fact]
        public async Task AdminController_GetSecurityReport_LoginDetails_Returns_Correct_Data()
        {
            // Arrange
            var user = _userClaimsPrincipalFake.GetAdminUserClaimsPrincipal();
            var mockSession = new Mock<ISession>();
            mockSession.Setup(x => x.Id).Returns("12345");
            var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user, Session = mockSession.Object } };

            var mockAzureAppSettings = new Mock<IOptions<AzureAppSettings>>();
            mockAzureAppSettings.Setup(x => x.Value)
                .Returns(new AzureAppSettings() { IsSessionIdStoredInCookie = false });

            var expected = new ReturnFile()
            {
                Bytes = new byte[200],
                FileName = "SecurityReport_LoginDetails",
                FileType = "csv"
            };

            _downloadSecurityReportLoginDetailsService.GetSecurityReportLoginDetails(Arg.Any<string>(),
                    Arg.Any<SecurityReportSearchType>(), Arg.Any<AzureFunctionHeaderDetails>())
                .Returns(expected);

            var controller = new AdminController(
                _securityService,
                _sessionProvider,
                _downloadSecurityReportByUpnService,
                _downloadSecurityReportLoginDetailsService,
                _downloadSecurityReportDetailedSearchesService);
            controller.ControllerContext = context;

            // Act
            var result = await controller.GetSecurityReport("logindetails", "001|Test", "LocalAuthority").ConfigureAwait(false) as FileContentResult;

            // Assert
            Assert.IsType<FileContentResult>(result);
            Assert.Equal(expected.Bytes.Length, result.FileContents.Length);
        }


        [Fact]
        public async Task AdminController_GetSecurityReport_Returns_NoContent_if_no_data()
        {
            // Arrange
            var user = _userClaimsPrincipalFake.GetUserClaimsPrincipal();
            var mockSession = new Mock<ISession>();
            mockSession.Setup(x => x.Id).Returns("12345");
            var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user, Session = mockSession.Object } };

            var mockAzureAppSettings = new Mock<IOptions<AzureAppSettings>>();
            mockAzureAppSettings.Setup(x => x.Value)
                .Returns(new AzureAppSettings() { IsSessionIdStoredInCookie = false });

            _downloadSecurityReportDetailedSearchesService.GetSecurityReportDetailedSearches(Arg.Any<string>(),
                    Arg.Any<SecurityReportSearchType>(), Arg.Any<AzureFunctionHeaderDetails>(), Arg.Any<bool>())
                .Returns(Task.FromResult<ReturnFile>(new ReturnFile()));

            var controller = new AdminController(
                _securityService,
                _sessionProvider,
                _downloadSecurityReportByUpnService,
                _downloadSecurityReportLoginDetailsService,
                _downloadSecurityReportDetailedSearchesService);
            controller.ControllerContext = context;

            // Act
            var result = await controller.GetSecurityReport("detailedsearches", "001", "LocalAuthority").ConfigureAwait(false) as NoContentResult;

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task AdminController_GetSecurityReport_Returns_NoContent_if_report_type_invalid()
        {
            // Arrange
            var user = _userClaimsPrincipalFake.GetUserClaimsPrincipal();
            var mockSession = new Mock<ISession>();
            mockSession.Setup(x => x.Id).Returns("12345");
            var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user, Session = mockSession.Object } };

            var mockAzureAppSettings = new Mock<IOptions<AzureAppSettings>>();
            mockAzureAppSettings.Setup(x => x.Value)
                .Returns(new AzureAppSettings() { IsSessionIdStoredInCookie = false });

            _downloadSecurityReportDetailedSearchesService.GetSecurityReportDetailedSearches(Arg.Any<string>(),
                    Arg.Any<SecurityReportSearchType>(), Arg.Any<AzureFunctionHeaderDetails>(), Arg.Any<bool>())
                .Returns(Task.FromResult<ReturnFile>(new ReturnFile()));

            var controller = new AdminController(
                _securityService,
                _sessionProvider,
                _downloadSecurityReportByUpnService,
                _downloadSecurityReportLoginDetailsService,
                _downloadSecurityReportDetailedSearchesService);
            controller.ControllerContext = context;

            // Act
            var result = await controller.GetSecurityReport("not a report type", "001", "LocalAuthority").ConfigureAwait(false) as NoContentResult;

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public void AdminController_SecurityReportsBySchoolConfirmationPost_Returns_Establishment_Redirect_To_Action()
        {
            // Arrange
            var user = new UserClaimsPrincipalFake().GetUserClaimsPrincipal();
            var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user } };
            var controller = GetAdminController();
            controller.ControllerContext = context;

            var model = new SecurityReportsBySchoolViewModel();
            model.SelectedConfirmationOption = "AnotherReport";

            // Act
            var result = controller.SecurityReportsBySchoolConfirmation(model);

            // Assert
            var viewResult = Assert.IsAssignableFrom<RedirectToActionResult>(result);
            Assert.True(viewResult.ControllerName.Equals("Admin"));
            Assert.True(viewResult.ActionName.Equals("SecurityReportsBySchoolEstablishmentSelection"));
        }

        [Fact]
        public void AdminController_SecurityReportsBySchoolConfirmationPost_Returns_School_Redirect_To_Action()
        {
            // Arrange
            var user = new UserClaimsPrincipalFake().GetUserClaimsPrincipal();
            var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user } };
            var controller = GetAdminController();
            controller.ControllerContext = context;

            var model = new SecurityReportsBySchoolViewModel();
            model.SelectedConfirmationOption = "ChangeReport";

            // Act
            var result = controller.SecurityReportsBySchoolConfirmation(model);

            // Assert
            var viewResult = Assert.IsAssignableFrom<RedirectToActionResult>(result);
            Assert.True(viewResult.ControllerName.Equals("Admin"));
            Assert.True(viewResult.ActionName.Equals("SecurityReportsBySchool"));
        }

        [Fact]
        public void AdminController_SecurityReportsBySchoolConfirmationPost_Returns_Dashboard_Redirect_To_Action()
        {
            // Arrange
            var user = new UserClaimsPrincipalFake().GetUserClaimsPrincipal();
            var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user } };
            var controller = GetAdminController();
            controller.ControllerContext = context;

            var model = new SecurityReportsBySchoolViewModel();
            model.SelectedConfirmationOption = "Admin";

            // Act
            var result = controller.SecurityReportsBySchoolConfirmation(model);

            // Assert
            var viewResult = Assert.IsAssignableFrom<RedirectToActionResult>(result);
            Assert.True(viewResult.ControllerName.Equals("Admin"));
            Assert.True(viewResult.ActionName.Equals("Index"));
        }

        [Fact]
        public void AdminController_SecurityReportsBySchoolConfirmationPost_Returns_Validation_Message_If_No_Option_Selected()
        {
            // Arrange
            var user = new UserClaimsPrincipalFake().GetUserClaimsPrincipal();
            var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user } };
            var controller = GetAdminController();
            controller.ControllerContext = context;

            var model = new SecurityReportsBySchoolViewModel();
            model.SelectedConfirmationOption = "";

            // Act
            var result = controller.SecurityReportsBySchoolConfirmation(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var viewModel = viewResult.Model as SecurityReportsBySchoolViewModel;
            Assert.NotNull(viewResult);
            Assert.NotNull(viewModel);
            Assert.Equal("../Admin/SecurityReports/SecurityReportsBySchoolConfirmation", viewResult.ViewName);
            Assert.True(controller.ViewData.ModelState["NoConfirmationSelection"].Errors.Count == 1);
        }

        [Fact]
        public async Task AdminController_SecurityReportsForYourOrganisation_DetailedSearches_Post_Returns_Correct_Data()
        {
            // Arrange
            var user = _userClaimsPrincipalFake.GetUserClaimsPrincipal();
            var mockSession = new Mock<ISession>();
            mockSession.Setup(x => x.Id).Returns("0");
            var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user, Session = mockSession.Object } };

            var mockAzureAppSettings = new Mock<IOptions<AzureAppSettings>>();
            mockAzureAppSettings.Setup(x => x.Value)
                .Returns(new AzureAppSettings() { IsSessionIdStoredInCookie = false });

            var expected = new ReturnFile()
            {
                Bytes = new byte[200],
                FileName = "SecurityReport_DetailedSearches",
                FileType = "csv"
            };
            _downloadSecurityReportDetailedSearchesService.GetSecurityReportDetailedSearches(Arg.Any<string>(),
                    Arg.Any<SecurityReportSearchType>(), Arg.Any<AzureFunctionHeaderDetails>(), Arg.Any<bool>())
                .Returns(expected);

            var controller = new AdminController(
                _securityService,
                _sessionProvider,
                _downloadSecurityReportByUpnService,
                _downloadSecurityReportLoginDetailsService,
                _downloadSecurityReportDetailedSearchesService);
            controller.ControllerContext = context;

            var model = new SecurityReportsForYourOrganisationViewModel();
            model.DocumentId = "0";

            // Act
            var result = await controller.SecurityReportsForYourOrganisation(model).ConfigureAwait(false) as FileContentResult;

            // Assert
            Assert.IsType<FileContentResult>(result);
            Assert.Equal(expected.Bytes.Length, result.FileContents.Length);
        }

        [Fact]
        public async Task AdminController_SecurityReportsForYourOrganisation_LoginDetails_Post_Returns_Correct_Data()
        {
            // Arrange
            var user = _userClaimsPrincipalFake.GetUserClaimsPrincipal();
            var mockSession = new Mock<ISession>();
            mockSession.Setup(x => x.Id).Returns("1");
            var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user, Session = mockSession.Object } };

            var mockAzureAppSettings = new Mock<IOptions<AzureAppSettings>>();
            mockAzureAppSettings.Setup(x => x.Value)
                .Returns(new AzureAppSettings() { IsSessionIdStoredInCookie = false });

            var expected = new ReturnFile()
            {
                Bytes = new byte[200],
                FileName = "SecurityReport_LoginDetails",
                FileType = "csv"
            };
            _downloadSecurityReportLoginDetailsService.GetSecurityReportLoginDetails(Arg.Any<string>(),
                    Arg.Any<SecurityReportSearchType>(), Arg.Any<AzureFunctionHeaderDetails>())
                .Returns(expected);

            var controller = new AdminController(
                 _securityService,
                 _sessionProvider,
                 _downloadSecurityReportByUpnService,
                 _downloadSecurityReportLoginDetailsService,
                 _downloadSecurityReportDetailedSearchesService);
            controller.ControllerContext = context;

            var model = new SecurityReportsForYourOrganisationViewModel();
            model.DocumentId = "1";

            // Act
            var result = await controller.SecurityReportsForYourOrganisation(model).ConfigureAwait(false) as FileContentResult;

            // Assert
            Assert.IsType<FileContentResult>(result);
            Assert.Equal(expected.Bytes.Length, result.FileContents.Length);
        }

        [Fact]
        public async Task AdminController_SecurityReportsForYourOrganisation_DetailedSearches_Post_Returns_Validation_Message_If_DocumentID_Is_Null()
        {
            // Arrange
            var user = _userClaimsPrincipalFake.GetUserClaimsPrincipal();
            var mockSession = new Mock<ISession>();
            mockSession.Setup(x => x.Id).Returns("12345");
            var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user, Session = mockSession.Object } };

            var mockAzureAppSettings = new Mock<IOptions<AzureAppSettings>>();
            mockAzureAppSettings.Setup(x => x.Value)
                .Returns(new AzureAppSettings() { IsSessionIdStoredInCookie = false });

            var expected = new ReturnFile()
            {
                Bytes = new byte[200],
                FileName = "SecurityReport_DetailedSearches",
                FileType = "csv"
            };
            _downloadSecurityReportDetailedSearchesService.GetSecurityReportDetailedSearches(Arg.Any<string>(),
                    Arg.Any<SecurityReportSearchType>(), Arg.Any<AzureFunctionHeaderDetails>(), Arg.Any<bool>())
                .Returns(expected);

            var controller = GetAdminController();
            controller.ControllerContext = context;

            var model = new SecurityReportsForYourOrganisationViewModel();
            model.DocumentId = null;

            // Act
            var result = await controller.SecurityReportsForYourOrganisation(model).ConfigureAwait(false) as ViewResult;

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var viewModel = viewResult.Model as SecurityReportsForYourOrganisationViewModel;
            Assert.NotNull(viewResult);
            Assert.NotNull(viewModel);
            Assert.Equal("../Admin/SecurityReports/SecurityReportsForYourOrganisation", viewResult.ViewName);
            Assert.True(controller.ViewData.ModelState["NoOrganisationalReportSelected"].Errors.Count == 1);
        }

        [Fact]
        public async Task AdminController_SecurityReportsForYourOrganisation_DetailedSearches_Post_Returns_Validation_Message_If_DocumentID_Is_Invalid()
        {
            // Arrange
            var user = _userClaimsPrincipalFake.GetUserClaimsPrincipal();
            var mockSession = new Mock<ISession>();
            mockSession.Setup(x => x.Id).Returns("12345");
            var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user, Session = mockSession.Object } };

            var mockAzureAppSettings = new Mock<IOptions<AzureAppSettings>>();
            mockAzureAppSettings.Setup(x => x.Value)
                .Returns(new AzureAppSettings() { IsSessionIdStoredInCookie = false });

            var expected = new ReturnFile()
            {
                Bytes = new byte[200],
                FileName = "SecurityReport_DetailedSearches",
                FileType = "csv"
            };
            _downloadSecurityReportDetailedSearchesService.GetSecurityReportDetailedSearches(Arg.Any<string>(),
                    Arg.Any<SecurityReportSearchType>(), Arg.Any<AzureFunctionHeaderDetails>(), Arg.Any<bool>())
                .Returns(expected);

            var controller = GetAdminController();
            controller.ControllerContext = context;

            var model = new SecurityReportsForYourOrganisationViewModel();
            model.DocumentId = "12345";

            // Act
            var result = await controller.SecurityReportsForYourOrganisation(model).ConfigureAwait(false) as ViewResult;

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var viewModel = viewResult.Model as SecurityReportsForYourOrganisationViewModel;
            Assert.NotNull(viewResult);
            Assert.NotNull(viewModel);
            Assert.Equal("../Admin/SecurityReports/SecurityReportsForYourOrganisation", viewResult.ViewName);
            Assert.True(controller.ViewData.ModelState["NoDataForOrganisationalDownloadExists"].Errors.Count == 1);
        }
    }
}
