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
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Security.Claims;

namespace DfE.GIAP.Web.Tests.Controllers.Admin;

[Trait("Category", "Admin Controller Unit Tests")]
public class AdminControllerTests : IClassFixture<UserClaimsPrincipalFake>
{
    private readonly UserClaimsPrincipalFake _userClaimsPrincipalFake;
    private readonly ISecurityService _securityService = Substitute.For<ISecurityService>();
    private readonly ISessionProvider _sessionProvider = Substitute.For<ISessionProvider>();

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
        ClaimsPrincipal user = new UserClaimsPrincipalFake().GetAdminUserClaimsPrincipal();
        ControllerContext context = new()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };
        AdminController controller = GetAdminController();
        controller.ControllerContext = context;

        // Act
        IActionResult result = controller.Index();

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        Assert.Equal("../Admin/Index", viewResult.ViewName);
    }

    [Fact]
    public void AdminController_DashboardOptions_Returns_ManageDocuments_Redirect_To_Action()
    {
        // Arrange
        ClaimsPrincipal user = new UserClaimsPrincipalFake().GetUserClaimsPrincipal();
        ControllerContext context = new()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };
        AdminController controller = GetAdminController();
        controller.ControllerContext = context;

        AdminViewModel model = new()
        {
            SelectedAdminOption = "ManageDocuments"
        };

        // Act
        IActionResult result = controller.AdminOptions(model);

        // Assert
        RedirectToActionResult viewResult = Assert.IsType<RedirectToActionResult>(result, exactMatch: false);
        Assert.Equal("ManageDocuments", viewResult.ControllerName);
        Assert.Equal("ManageDocuments", viewResult.ActionName);
    }

    [Fact]
    public void AdminController_DashboardOptions_Returns_SecurityReportsByUpnUln_Redirect_To_Action()
    {
        // Arrange
        ClaimsPrincipal user = new UserClaimsPrincipalFake().GetUserClaimsPrincipal();
        ControllerContext context = new()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };
        AdminController controller = GetAdminController();
        controller.ControllerContext = context;

        AdminViewModel model = new()
        {
            SelectedAdminOption = "DownloadSecurityReportsByPupilOrStudent"
        };

        // Act
        IActionResult result = controller.AdminOptions(model);

        // Assert
        RedirectToActionResult viewResult = Assert.IsType<RedirectToActionResult>(result, exactMatch: false);
        Assert.Equal("SecurityReportByPupilStudentRecord", viewResult.ControllerName);
        Assert.Equal("SecurityReportsByUpnUln", viewResult.ActionName);
    }

    [Fact]
    public void AdminController_DashboardOptions_Returns_SecurityReportsForYourOrganisation_Redirect_To_Action()
    {
        // Arrange
        ClaimsPrincipal user = new UserClaimsPrincipalFake().GetUserClaimsPrincipal();
        ControllerContext context = new()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };
        AdminController controller = GetAdminController();
        controller.ControllerContext = context;

        AdminViewModel model = new()
        {
            SelectedAdminOption = "DownloadSecurityReportsByOrganisation"
        };

        // Act
        IActionResult result = controller.AdminOptions(model);

        // Assert
        RedirectToActionResult viewResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Admin", viewResult.ControllerName);
        Assert.Equal("SecurityReportsForYourOrganisation", viewResult.ActionName);
    }

    [Fact]
    public void AdminController_DashboardOptions_Returns_DownloadSecurityReportsBySchool_Admin_Redirect_To_Action()
    {
        // Arrange
        ClaimsPrincipal user = new UserClaimsPrincipalFake().GetAdminUserClaimsPrincipal();
        ControllerContext context = new()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };
        AdminController controller = GetAdminController();
        controller.ControllerContext = context;

        AdminViewModel model = new()
        {
            SelectedAdminOption = "DownloadSecurityReportsBySchool"
        };

        // Act
        IActionResult result = controller.AdminOptions(model);

        // Assert
        RedirectToActionResult viewResult = Assert.IsType<RedirectToActionResult>(result, exactMatch: false);
        Assert.Equal("Admin", viewResult.ControllerName);
        Assert.Equal("SchoolCollegeDownloadOptions", viewResult.ActionName);
    }

    [Fact]
    public void AdminController_DashboardOptions_Returns_DownloadSecurityReportsBySchool_NonAdmin_Redirect_To_Action()
    {
        // Arrange
        ClaimsPrincipal user = new UserClaimsPrincipalFake().GetUserClaimsPrincipal();
        ControllerContext context = new()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };
        AdminController controller = GetAdminController();
        controller.ControllerContext = context;

        AdminViewModel model = new()
        {
            SelectedAdminOption = "DownloadSecurityReportsBySchool"
        };

        // Act
        IActionResult result = controller.AdminOptions(model);

        // Assert
        RedirectToActionResult viewResult = Assert.IsType<RedirectToActionResult>(result, exactMatch: false);
        Assert.Equal("Admin", viewResult.ControllerName);
        Assert.Equal("SecurityReportsBySchool", viewResult.ActionName);
    }

    [Fact]
    public void AdminController_DashboardOptions_Returns_ValidationMessage_If_No_Selection_Made()
    {
        // Arrange
        ClaimsPrincipal user = new UserClaimsPrincipalFake().GetAdminUserClaimsPrincipal();
        ControllerContext context = new()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };
        AdminController controller = GetAdminController();
        controller.ControllerContext = context;

        AdminViewModel model = new()
        {
            SelectedAdminOption = null
        };

        // Act
        IActionResult result = controller.AdminOptions(model);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        AdminViewModel viewModel = viewResult.Model as AdminViewModel;
        Assert.NotNull(viewModel);
        Assert.Equal("../Admin/Index", viewResult.ViewName);
        Assert.Single(controller.ViewData.ModelState["NoAdminSelection"].Errors);
    }

    [Fact]
    public void AdminController_SchoolCollegeDownloadOptionsAdminGet_Renders_Correct_View()
    {
        // Arrange
        ClaimsPrincipal user = new UserClaimsPrincipalFake().GetAdminUserClaimsPrincipal();
        ControllerContext context = new()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };
        AdminController controller = GetAdminController();
        controller.ControllerContext = context;

        // Act
        IActionResult result = controller.SchoolCollegeDownloadOptions();

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        Assert.Equal("../Admin/SecurityReports/SchoolCollegeDownloadOptions", viewResult.ViewName);
    }

    [Fact]
    public void AdminController_SchoolCollegeDownloadOptions_SecurityReportsBySchool_Redirect_To_Action()
    {
        // Arrange
        ClaimsPrincipal user = new UserClaimsPrincipalFake().GetAdminUserClaimsPrincipal();
        ControllerContext context = new()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };
        AdminController controller = GetAdminController();
        controller.ControllerContext = context;

        AdminViewModel model = new()
        {
            SelectedOrganisationOption = "AcademyTrust"
        };

        // Act
        IActionResult result = controller.SchoolCollegeDownloadOptions(model);

        // Assert
        RedirectToActionResult viewResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Admin", viewResult.ControllerName);
        Assert.Equal("SecurityReportsBySchool", viewResult.ActionName);
    }

    [Fact]
    public void AdminController_SchoolCollegeDownloadOptions_Returns_Validation_Message_If_No_Selection_Made()
    {
        // Arrange
        ClaimsPrincipal user = new UserClaimsPrincipalFake().GetAdminUserClaimsPrincipal();
        ControllerContext context = new()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };
        AdminController controller = GetAdminController();
        controller.ControllerContext = context;

        AdminViewModel model = new()
        {
            SelectedOrganisationOption = null
        };

        // Act
        IActionResult result = controller.SchoolCollegeDownloadOptions(model);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        Assert.Equal("../Admin/SecurityReports/SchoolCollegeDownloadOptions", viewResult.ViewName);
        Assert.Single(controller.ViewData.ModelState["NoOrganisationSelection"].Errors);
    }

    [Fact]
    public async Task AdminController_SecurityReportsBySchoolGet_Renders_Correct_View()
    {
        // Arrange
        ClaimsPrincipal user = new UserClaimsPrincipalFake().GetAdminUserClaimsPrincipal();
        ControllerContext context = new()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };
        AdminController controller = GetAdminController();
        controller.ControllerContext = context;

        // Act
        IActionResult result = await controller.SecurityReportsBySchool();

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result, exactMatch: false);
        Assert.NotNull(viewResult);
        Assert.Equal("../Admin/SecurityReports/SecurityReportsBySchool", viewResult.ViewName);
    }

    [Fact]
    public async Task AdminController_SecurityReportsBySchoolEstablishmentSelectionGet_Renders_Correct_View()
    {
        // Arrange
        ClaimsPrincipal user = new UserClaimsPrincipalFake().GetUserClaimsPrincipal();
        ControllerContext context = new()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };
        AdminController controller = GetAdminController();
        controller.ControllerContext = context;

        // Act
        IActionResult result = await controller.SecurityReportsBySchoolEstablishmentSelection();

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        Assert.Equal("../Admin/SecurityReports/SecurityReportsBySchoolEstablishmentSelection", viewResult.ViewName);
    }

    [Fact]
    public void AdminController_SecurityReportsBySchoolConfirmationGet_Renders_Correct_View()
    {
        // Arrange
        ClaimsPrincipal user = new UserClaimsPrincipalFake().GetUserClaimsPrincipal();
        ControllerContext context = new()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };
        AdminController controller = GetAdminController();
        controller.ControllerContext = context;

        // Act
        IActionResult result = controller.SecurityReportsBySchoolConfirmation();

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        Assert.Equal("../Admin/SecurityReports/SecurityReportsBySchoolConfirmation", viewResult.ViewName);
    }

    [Fact]
    public void AdminController_SecurityReportsForYourOrganisationGet_Renders_Correct_View()
    {
        // Arrange
        ClaimsPrincipal user = new UserClaimsPrincipalFake().GetUserClaimsPrincipal();
        ControllerContext context = new()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };
        AdminController controller = GetAdminController();
        controller.ControllerContext = context;

        // Act
        IActionResult result = controller.SecurityReportsForYourOrganisation();

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        Assert.Equal("../Admin/SecurityReports/SecurityReportsForYourOrganisation", viewResult.ViewName);
    }

    [Fact]
    public async Task AdminController_SecurityReportsBySchoolPost_Adds_ModelError_If_Neither_LA_or_AT_Selected()
    {
        // Arrange
        ClaimsPrincipal user = new UserClaimsPrincipalFake().GetAdminUserClaimsPrincipal();
        ControllerContext context = new()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };
        AdminController controller = GetAdminController();
        controller.ControllerContext = context;

        SecurityReportsBySchoolViewModel model = new()
        {
            SelectedOrganisationCode = null
        };

        // Act
        IActionResult result = await controller.SecurityReportsBySchool(model);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        Assert.Equal("../Admin/SecurityReports/SecurityReportsBySchool", viewResult.ViewName);
        Assert.Single(controller.ViewData.ModelState["NoOrganisationSelected"].Errors);
    }

    [Fact]
    public async Task AdminController_DownloadSecurityReportsBySchoolPost_Adds_ModelError_If_Neither_LA_or_AT_Selected_And_No_Establishment()
    {
        // Arrange
        ClaimsPrincipal user = new UserClaimsPrincipalFake().GetAdminUserClaimsPrincipal();
        ControllerContext context = new()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };
        AdminController controller = GetAdminController();
        controller.ControllerContext = context;

        SecurityReportsBySchoolViewModel model = new()
        {
            SelectedEstablishmentCode = null,
            SelectedOrganisationCode = null
        };

        // Act
        IActionResult result = await controller.DownloadSecurityReportsBySchool(model);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        Assert.Equal("../Admin/SecurityReports/SecurityReportsBySchoolEstablishmentSelection", viewResult.ViewName);
        Assert.Single(controller.ViewData.ModelState["NoOrganisationSelected"].Errors);
    }

    [Fact]
    public async Task AdminController_DownloadSecurityReportsBySchoolPost_Adds_ModelError_If_Both_LA_and_AT_Selected_And_No_Establishment()
    {
        // Arrange
        ClaimsPrincipal user = new UserClaimsPrincipalFake().GetAdminUserClaimsPrincipal();
        ControllerContext context = new()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };
        AdminController controller = GetAdminController();
        controller.ControllerContext = context;

        SecurityReportsBySchoolViewModel model = new()
        {
            SelectedEstablishmentCode = null,
            SelectedOrganisationCode = "Test LA"
        };

        // Act
        IActionResult result = await controller.DownloadSecurityReportsBySchool(model);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        Assert.Equal("../Admin/SecurityReports/SecurityReportsBySchoolEstablishmentSelection", viewResult.ViewName);
        Assert.False(controller.ViewData.ModelState.IsValid);
        ModelError modelError = Assert.Single(controller.ViewData.ModelState["NoEstablishmentSelected"].Errors);
        Assert.Equal(SecurityReportsConstants.NoEstablishmentSelected, modelError.ErrorMessage);
    }

    [Fact]
    public async Task AdminController_DownloadSecurityReportsBySchoolPost_Sets_Correct_Model_Properties_If_No_Establishment()
    {
        // Arrange
        ClaimsPrincipal user = new UserClaimsPrincipalFake().GetAdminUserClaimsPrincipal();
        ControllerContext context = new()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };
        AdminController controller = GetAdminController();
        controller.ControllerContext = context;

        SecurityReportsBySchoolViewModel model = new()
        {
            SelectedEstablishmentCode = null,
            SelectedOrganisationCode = "Test LA",
            SelectedReportType = "Test report type"
        };

        // Act
        IActionResult result = await controller.DownloadSecurityReportsBySchool(model);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        Assert.Equal("../Admin/SecurityReports/SecurityReportsBySchoolEstablishmentSelection", viewResult.ViewName);
        Assert.Equal(model.SelectedReportType, controller.ViewBag.SelectedReportType);
        Assert.Equal(model.SelectedOrganisationCode, controller.ViewBag.SelectedOrganisationCode);
        ModelError error = Assert.Single(controller.ViewData.ModelState["NoEstablishmentSelected"].Errors);
        Assert.Equal(SecurityReportsConstants.NoEstablishmentSelected, error.ErrorMessage);
    }

    [Fact]
    public async Task AdminController_DownloadSecurityReportsBySchoolPost_Returns_Correct_Data()
    {
        // Arrange
        ClaimsPrincipal user = _userClaimsPrincipalFake.GetUserClaimsPrincipal();
        Mock<ISession> mockSession = new();
        mockSession.Setup(x => x.Id).Returns("12345");
        ControllerContext context = new()
        {
            HttpContext = new DefaultHttpContext() { User = user, Session = mockSession.Object }
        };

        Mock<IOptions<AzureAppSettings>> mockAzureAppSettings = new();
        mockAzureAppSettings.Setup(x => x.Value)
            .Returns(new AzureAppSettings() { IsSessionIdStoredInCookie = false });

        ReturnFile expected = new()
        {
            Bytes = new byte[200],
            FileName = "Test-DownloadSecurityReport-ByURN",
            FileType = "csv"
        };

        _downloadSecurityReportLoginDetailsService.GetSecurityReportLoginDetails(Arg.Any<string>(),
                Arg.Any<SecurityReportSearchType>(), Arg.Any<AzureFunctionHeaderDetails>())
            .Returns(expected);

        AdminController controller = new(
            _securityService,
            _sessionProvider,
            _downloadSecurityReportByUpnService,
            _downloadSecurityReportLoginDetailsService,
            _downloadSecurityReportDetailedSearchesService)
        {
            ControllerContext = context
        };

        SecurityReportsBySchoolViewModel model = new()
        {
            SelectedEstablishmentCode = "123456",
            SelectedOrganisationCode = "Test LA",
            SelectedReportType = "LoginDetails"
        };

        // Act
        IActionResult result = await controller.DownloadSecurityReportsBySchool(model);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        SecurityReportsBySchoolViewModel viewModel = viewResult.Model as SecurityReportsBySchoolViewModel;
        Assert.NotNull(viewModel);
        Assert.True(viewModel.ProcessDownload);
        Assert.Equal("../Admin/SecurityReports/SecurityReportsBySchoolEstablishmentSelection", viewResult.ViewName);
    }

    [Fact]
    public async Task AdminController_DownloadLoginDetailsSecurityReportsBySchool_By_UniqueReferenceNumber_Post_Returns_Correct_Data()
    {
        // Arrange
        ClaimsPrincipal user = _userClaimsPrincipalFake.GetUserClaimsPrincipal();
        Mock<ISession> mockSession = new();
        mockSession.Setup(x => x.Id).Returns("12345");

        ControllerContext context = new()
        {
            HttpContext = new DefaultHttpContext() { User = user, Session = mockSession.Object }
        };

        Mock<IOptions<AzureAppSettings>> mockAzureAppSettings = new();
        mockAzureAppSettings.Setup(x => x.Value)
            .Returns(new AzureAppSettings() { IsSessionIdStoredInCookie = false });

        ReturnFile expected = new()
        {
            Bytes = new byte[200],
            FileName = "Test-DownloadSecurityReport-ByURN",
            FileType = "csv"
        };

        _downloadSecurityReportLoginDetailsService.GetSecurityReportLoginDetails(Arg.Any<string>(),
                Arg.Any<SecurityReportSearchType>(), Arg.Any<AzureFunctionHeaderDetails>())
            .Returns(expected);

        AdminController controller = new(
            _securityService,
            _sessionProvider,
            _downloadSecurityReportByUpnService,
            _downloadSecurityReportLoginDetailsService,
            _downloadSecurityReportDetailedSearchesService)
        {
            ControllerContext = context
        };

        SecurityReportsBySchoolViewModel model = new()
        {
            SelectedEstablishmentCode = "123456",
            SelectedOrganisationCode = null,
            SelectedReportType = "LoginDetails"
        };

        // Act
        IActionResult result = await controller.DownloadSecurityReportsBySchool(model);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        SecurityReportsBySchoolViewModel viewModel = viewResult.Model as SecurityReportsBySchoolViewModel;
        Assert.NotNull(viewModel);
        Assert.True(viewModel.ProcessDownload);
        Assert.Equal("../Admin/SecurityReports/SecurityReportsBySchoolEstablishmentSelection", viewResult.ViewName);
    }

    [Fact]
    public async Task AdminController_DownloadSecurityReportsBySchool_By_SATApprover_Post_Returns_Correct_Data()
    {
        // Arrange
        ClaimsPrincipal user = _userClaimsPrincipalFake.GetSATApproverClaimsPrincipal();
        Mock<ISession> mockSession = new();
        mockSession.Setup(x => x.Id).Returns("12345");

        ControllerContext context = new()
        {
            HttpContext = new DefaultHttpContext() { User = user, Session = mockSession.Object }
        };

        Mock<IOptions<AzureAppSettings>> mockAzureAppSettings = new();
        mockAzureAppSettings.Setup(x => x.Value)
            .Returns(new AzureAppSettings() { IsSessionIdStoredInCookie = false });

        ReturnFile expected = new()
        {
            Bytes = new byte[200],
            FileName = "Test-DownloadSecurityReport-ByUPN",
            FileType = "csv"
        };

        _downloadSecurityReportLoginDetailsService.GetSecurityReportLoginDetails(Arg.Any<string>(),
                Arg.Any<SecurityReportSearchType>(), Arg.Any<AzureFunctionHeaderDetails>())
            .Returns(expected);

        AdminController controller = new(
            _securityService,
            _sessionProvider,
            _downloadSecurityReportByUpnService,
            _downloadSecurityReportLoginDetailsService,
            _downloadSecurityReportDetailedSearchesService)
        {
            ControllerContext = context
        };

        SecurityReportsBySchoolViewModel model = new()
        {
            SelectedOrganisationCode = "12345",
            SelectedEstablishmentCode = "6789",
            SelectedReportType = "LoginDetails"
        };

        // Act
        IActionResult result = await controller.DownloadSecurityReportsBySchool(model);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        SecurityReportsBySchoolViewModel viewModel = viewResult.Model as SecurityReportsBySchoolViewModel;
        Assert.NotNull(viewModel);
        Assert.True(viewModel.ProcessDownload);
        Assert.Equal("../Admin/SecurityReports/SecurityReportsBySchoolEstablishmentSelection", viewResult.ViewName);
    }


    [Fact]
    public async Task AdminController_DownloadSecurityReportsBySchool_DetailedSearches_By_UniqueReferenceNumber_Post_Returns_Correct_Data()
    {
        // Arrange
        ClaimsPrincipal user = _userClaimsPrincipalFake.GetUserClaimsPrincipal();
        Mock<ISession> mockSession = new();
        mockSession.Setup(x => x.Id).Returns("12345");

        ControllerContext context = new()
        {
            HttpContext = new DefaultHttpContext() { User = user, Session = mockSession.Object }
        };

        Mock<IOptions<AzureAppSettings>> mockAzureAppSettings = new();
        mockAzureAppSettings.Setup(x => x.Value)
            .Returns(new AzureAppSettings() { IsSessionIdStoredInCookie = false });

        ReturnFile expected = new()
        {
            Bytes = new byte[200],
            FileName = "SecurityReport_DetailedSearches",
            FileType = "csv"
        };

        _downloadSecurityReportDetailedSearchesService.GetSecurityReportDetailedSearches(Arg.Any<string>(),
                Arg.Any<SecurityReportSearchType>(), Arg.Any<AzureFunctionHeaderDetails>(), Arg.Any<bool>())
            .Returns(expected);

        AdminController controller = new(
             _securityService,
             _sessionProvider,
             _downloadSecurityReportByUpnService,
             _downloadSecurityReportLoginDetailsService,
             _downloadSecurityReportDetailedSearchesService)
        {
            ControllerContext = context
        };

        SecurityReportsBySchoolViewModel model = new()
        {
            SelectedEstablishmentCode = "123456",
            SelectedOrganisationCode = null,
            SelectedReportType = "DetailedSearches"
        };

        // Act
        IActionResult result = await controller.DownloadSecurityReportsBySchool(model);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        SecurityReportsBySchoolViewModel viewModel = viewResult.Model as SecurityReportsBySchoolViewModel;
        Assert.NotNull(viewModel);
        Assert.True(viewModel.ProcessDownload);
        Assert.Equal("../Admin/SecurityReports/SecurityReportsBySchoolEstablishmentSelection", viewResult.ViewName);
    }

    [Fact]
    public async Task AdminController_DownloadSecurityReportsBySchool_DetailedSearches_By_SATApprover_Post_Returns_Correct_Data()
    {
        // Arrange
        ClaimsPrincipal user = _userClaimsPrincipalFake.GetSATApproverClaimsPrincipal();

        Mock<ISession> mockSession = new();
        mockSession.Setup(x => x.Id).Returns("12345");

        ControllerContext context = new()
        {
            HttpContext = new DefaultHttpContext() { User = user, Session = mockSession.Object }
        };

        Mock<IOptions<AzureAppSettings>> mockAzureAppSettings = new();
        mockAzureAppSettings.Setup(x => x.Value)
            .Returns(new AzureAppSettings() { IsSessionIdStoredInCookie = false });

        ReturnFile expected = new()
        {
            Bytes = new byte[200],
            FileName = "SecurityReport_DetailedSearches",
            FileType = "csv"
        };
        _downloadSecurityReportDetailedSearchesService.GetSecurityReportDetailedSearches(Arg.Any<string>(),
                Arg.Any<SecurityReportSearchType>(), Arg.Any<AzureFunctionHeaderDetails>(), Arg.Any<bool>())
            .Returns(expected);

        AdminController controller = new(
             _securityService,
             _sessionProvider,
             _downloadSecurityReportByUpnService,
             _downloadSecurityReportLoginDetailsService,
             _downloadSecurityReportDetailedSearchesService)
        {
            ControllerContext = context
        };

        SecurityReportsBySchoolViewModel model = new()
        {
            SelectedOrganisationCode = "12345",
            SelectedEstablishmentCode = "6789",
            SelectedReportType = "DetailedSearches"
        };

        // Act
        IActionResult result = await controller.DownloadSecurityReportsBySchool(model);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        SecurityReportsBySchoolViewModel viewModel = viewResult.Model as SecurityReportsBySchoolViewModel;
        Assert.NotNull(viewResult);
        Assert.NotNull(viewModel);
        Assert.True(viewModel.ProcessDownload);
        Assert.Equal("../Admin/SecurityReports/SecurityReportsBySchoolEstablishmentSelection", viewResult.ViewName);
    }

    [Fact]
    public async Task AdminController_GetSecurityReport_Returns_Correct_Data()
    {
        // Arrange
        ClaimsPrincipal user = _userClaimsPrincipalFake.GetAdminUserClaimsPrincipal();
        Mock<ISession> mockSession = new();
        mockSession.Setup(x => x.Id).Returns("12345");

        ControllerContext context = new()
        {
            HttpContext = new DefaultHttpContext() { User = user, Session = mockSession.Object }
        };

        Mock<IOptions<AzureAppSettings>> mockAzureAppSettings = new();
        mockAzureAppSettings.Setup(x => x.Value)
            .Returns(new AzureAppSettings() { IsSessionIdStoredInCookie = false });

        ReturnFile expected = new()
        {
            Bytes = new byte[200],
            FileName = "SecurityReport_DetailedSearches",
            FileType = "csv"
        };
        _downloadSecurityReportDetailedSearchesService.GetSecurityReportDetailedSearches(Arg.Any<string>(),
                Arg.Any<SecurityReportSearchType>(), Arg.Any<AzureFunctionHeaderDetails>(), Arg.Any<bool>())
            .Returns(expected);

        AdminController controller = new(
             _securityService,
             _sessionProvider,
             _downloadSecurityReportByUpnService,
             _downloadSecurityReportLoginDetailsService,
             _downloadSecurityReportDetailedSearchesService)
        {
            ControllerContext = context
        };

        // Act
        IActionResult result = await controller.GetSecurityReport("detailedsearches", "001", "LocalAuthority");

        // Assert
        FileContentResult fileContentResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal(expected.Bytes.Length, fileContentResult.FileContents.Length);
    }

    [Fact]
    public async Task AdminController_GetSecurityReport_Returns_Correct_Data_For_SAT_Approver()
    {
        // Arrange
        ClaimsPrincipal user = _userClaimsPrincipalFake.GetSATApproverClaimsPrincipal();
        Mock<ISession> mockSession = new();
        mockSession.Setup(x => x.Id).Returns("12345");

        ControllerContext context = new()
        {
            HttpContext = new DefaultHttpContext() { User = user, Session = mockSession.Object }
        };

        Mock<IOptions<AzureAppSettings>> mockAzureAppSettings = new();
        mockAzureAppSettings.Setup(x => x.Value)
            .Returns(new AzureAppSettings() { IsSessionIdStoredInCookie = false });

        List<Establishment> expectedEstablishments =
        [
            new()
            {
                Name = "Test_SAT",
                URN = "013"
            }
        ];

        ReturnFile expected = new()
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

        AdminController controller = new(
            _securityService,
            _sessionProvider,
            _downloadSecurityReportByUpnService,
            _downloadSecurityReportLoginDetailsService,
            _downloadSecurityReportDetailedSearchesService)
        {
            ControllerContext = context
        };

        // Act
        IActionResult result = await controller.GetSecurityReport("detailedsearches", "013", "LocalAuthority");

        // Assert
        FileContentResult fileContentResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal(expected.Bytes.Length, fileContentResult.FileContents.Length);
    }

    [Fact]
    public async Task AdminController_GetSecurityReport_Returns_Correct_Data_For_LA_Approver()
    {
        // Arrange
        ClaimsPrincipal user = _userClaimsPrincipalFake.GetLAApproverClaimsPrincipal();
        Mock<ISession> mockSession = new();
        mockSession.Setup(x => x.Id).Returns("12345");

        ControllerContext context = new()
        {
            HttpContext = new DefaultHttpContext() { User = user, Session = mockSession.Object }
        };

        Mock<IOptions<AzureAppSettings>> mockAzureAppSettings = new();
        mockAzureAppSettings.Setup(x => x.Value)
            .Returns(new AzureAppSettings() { IsSessionIdStoredInCookie = false });

        List<Establishment> expectedEstablishments = [
            new()
            {
                Name = "Test_LA",
                URN = "002"
            }
        ];

        ReturnFile expected = new()
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

        AdminController controller = new(
             _securityService,
             _sessionProvider,
             _downloadSecurityReportByUpnService,
             _downloadSecurityReportLoginDetailsService,
             _downloadSecurityReportDetailedSearchesService)
        {
            ControllerContext = context
        };

        // Act
        IActionResult result = await controller.GetSecurityReport("detailedsearches", "002", "LocalAuthority");

        // Assert
        FileContentResult fileContentResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal(expected.Bytes.Length, fileContentResult.FileContents.Length);
    }

    [Fact]
    public async Task AdminController_GetSecurityReport_Returns_Correct_Data_For_FE_Approver()
    {
        // Arrange
        ClaimsPrincipal user = _userClaimsPrincipalFake.GetFEApproverClaimsPrincipal();
        Mock<ISession> mockSession = new();
        mockSession.Setup(x => x.Id).Returns("12345");

        ControllerContext context = new()
        {
            HttpContext = new DefaultHttpContext() { User = user, Session = mockSession.Object }
        };

        Mock<IOptions<AzureAppSettings>> mockAzureAppSettings = new();
        mockAzureAppSettings.Setup(x => x.Value)
            .Returns(new AzureAppSettings() { IsSessionIdStoredInCookie = false });

        List<Establishment> expectedEstablishments = [
            new()
            {
                Name = "Test_FE",
                URN = "001"
            }
        ];

        ReturnFile expected = new()
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

        AdminController controller = new(
            _securityService,
            _sessionProvider,
            _downloadSecurityReportByUpnService,
            _downloadSecurityReportLoginDetailsService,
            _downloadSecurityReportDetailedSearchesService)
        {
            ControllerContext = context
        };

        // Act
        IActionResult result = await controller.GetSecurityReport("detailedsearches", "001", "LocalAuthority");

        // Assert
        FileContentResult fileContentResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal(expected.Bytes.Length, fileContentResult.FileContents.Length);
    }

    [Fact]
    public async Task AdminController_GetSecurityReport_Returns_Correct_Data_For_Organisation_User()
    {
        // Arrange
        ClaimsPrincipal user = _userClaimsPrincipalFake.GetUserClaimsPrincipal();
        Mock<ISession> mockSession = new();
        mockSession.Setup(x => x.Id).Returns("12345");

        ControllerContext context = new()
        {
            HttpContext = new DefaultHttpContext() { User = user, Session = mockSession.Object }
        };

        Mock<IOptions<AzureAppSettings>> mockAzureAppSettings = new();
        mockAzureAppSettings.Setup(x => x.Value)
            .Returns(new AzureAppSettings() { IsSessionIdStoredInCookie = false });

        List<Establishment> expectedEstablishments =
        [
            new()
            {
                Name = "Test_Org",
                URN = "121"
            }
        ];

        ReturnFile expected = new()
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

        AdminController controller = new(
            _securityService,
            _sessionProvider,
            _downloadSecurityReportByUpnService,
            _downloadSecurityReportLoginDetailsService,
            _downloadSecurityReportDetailedSearchesService)
        {
            ControllerContext = context
        };

        // Act
        IActionResult result = await controller.GetSecurityReport("detailedsearches", "121", "LocalAuthority");

        // Assert
        FileContentResult fileContentResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal(expected.Bytes.Length, fileContentResult.FileContents.Length);
    }

    [Fact]
    public async Task AdminController_GetSecurityReport_LoginDetails_Returns_Correct_Data()
    {
        // Arrange
        ClaimsPrincipal user = _userClaimsPrincipalFake.GetAdminUserClaimsPrincipal();
        Mock<ISession> mockSession = new();
        mockSession.Setup(x => x.Id).Returns("12345");

        ControllerContext context = new()
        {
            HttpContext = new DefaultHttpContext() { User = user, Session = mockSession.Object }
        };

        Mock<IOptions<AzureAppSettings>> mockAzureAppSettings = new();
        mockAzureAppSettings.Setup(x => x.Value)
            .Returns(new AzureAppSettings() { IsSessionIdStoredInCookie = false });

        ReturnFile expected = new()
        {
            Bytes = new byte[200],
            FileName = "SecurityReport_LoginDetails",
            FileType = "csv"
        };

        _downloadSecurityReportLoginDetailsService.GetSecurityReportLoginDetails(Arg.Any<string>(),
                Arg.Any<SecurityReportSearchType>(), Arg.Any<AzureFunctionHeaderDetails>())
            .Returns(expected);

        AdminController controller = new(
            _securityService,
            _sessionProvider,
            _downloadSecurityReportByUpnService,
            _downloadSecurityReportLoginDetailsService,
            _downloadSecurityReportDetailedSearchesService)
        {
            ControllerContext = context
        };

        // Act
        IActionResult result = await controller.GetSecurityReport("logindetails", "001|Test", "LocalAuthority");

        // Assert
        FileContentResult fileContentResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal(expected.Bytes.Length, fileContentResult.FileContents.Length);
    }


    [Fact]
    public async Task AdminController_GetSecurityReport_Returns_NoContent_if_no_data()
    {
        // Arrange
        ClaimsPrincipal user = _userClaimsPrincipalFake.GetUserClaimsPrincipal();
        Mock<ISession> mockSession = new();
        mockSession.Setup(x => x.Id).Returns("12345");

        ControllerContext context = new()
        {
            HttpContext = new DefaultHttpContext() { User = user, Session = mockSession.Object }
        };

        Mock<IOptions<AzureAppSettings>> mockAzureAppSettings = new();
        mockAzureAppSettings.Setup(x => x.Value)
            .Returns(new AzureAppSettings() { IsSessionIdStoredInCookie = false });

        _downloadSecurityReportDetailedSearchesService.GetSecurityReportDetailedSearches(Arg.Any<string>(),
                Arg.Any<SecurityReportSearchType>(), Arg.Any<AzureFunctionHeaderDetails>(), Arg.Any<bool>())
            .Returns(Task.FromResult(new ReturnFile()));

        AdminController controller = new(
            _securityService,
            _sessionProvider,
            _downloadSecurityReportByUpnService,
            _downloadSecurityReportLoginDetailsService,
            _downloadSecurityReportDetailedSearchesService)
        {
            ControllerContext = context
        };

        // Act
        IActionResult result = await controller.GetSecurityReport("detailedsearches", "001", "LocalAuthority");

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task AdminController_GetSecurityReport_Returns_NoContent_if_report_type_invalid()
    {
        // Arrange
        ClaimsPrincipal user = _userClaimsPrincipalFake.GetUserClaimsPrincipal();
        Mock<ISession> mockSession = new();
        mockSession.Setup(x => x.Id).Returns("12345");

        ControllerContext context = new()
        {
            HttpContext = new DefaultHttpContext() { User = user, Session = mockSession.Object }
        };

        Mock<IOptions<AzureAppSettings>> mockAzureAppSettings = new();
        mockAzureAppSettings.Setup(x => x.Value)
            .Returns(new AzureAppSettings() { IsSessionIdStoredInCookie = false });

        _downloadSecurityReportDetailedSearchesService.GetSecurityReportDetailedSearches(Arg.Any<string>(),
                Arg.Any<SecurityReportSearchType>(), Arg.Any<AzureFunctionHeaderDetails>(), Arg.Any<bool>())
            .Returns(Task.FromResult(new ReturnFile()));

        AdminController controller = new(
            _securityService,
            _sessionProvider,
            _downloadSecurityReportByUpnService,
            _downloadSecurityReportLoginDetailsService,
            _downloadSecurityReportDetailedSearchesService)
        {
            ControllerContext = context
        };

        // Act
        IActionResult result = await controller.GetSecurityReport("not a report type", "001", "LocalAuthority");

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public void AdminController_SecurityReportsBySchoolConfirmationPost_Returns_Establishment_Redirect_To_Action()
    {
        // Arrange
        ClaimsPrincipal user = new UserClaimsPrincipalFake().GetUserClaimsPrincipal();
        ControllerContext context = new()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };
        AdminController controller = GetAdminController();
        controller.ControllerContext = context;

        SecurityReportsBySchoolViewModel model = new()
        {
            SelectedConfirmationOption = "AnotherReport"
        };

        // Act
        IActionResult result = controller.SecurityReportsBySchoolConfirmation(model);

        // Assert
        RedirectToActionResult viewResult = Assert.IsType<RedirectToActionResult>(result, exactMatch: false);
        Assert.Equal("Admin", viewResult.ControllerName);
        Assert.Equal("SecurityReportsBySchoolEstablishmentSelection", viewResult.ActionName);
    }

    [Fact]
    public void AdminController_SecurityReportsBySchoolConfirmationPost_Returns_School_Redirect_To_Action()
    {
        // Arrange
        ClaimsPrincipal user = new UserClaimsPrincipalFake().GetUserClaimsPrincipal();
        ControllerContext context = new()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };
        AdminController controller = GetAdminController();
        controller.ControllerContext = context;

        SecurityReportsBySchoolViewModel model = new()
        {
            SelectedConfirmationOption = "ChangeReport"
        };

        // Act
        IActionResult result = controller.SecurityReportsBySchoolConfirmation(model);

        // Assert
        RedirectToActionResult viewResult = Assert.IsType<RedirectToActionResult>(result, exactMatch: false);
        Assert.Equal("Admin", viewResult.ControllerName);
        Assert.Equal("SecurityReportsBySchool", viewResult.ActionName);
    }

    [Fact]
    public void AdminController_SecurityReportsBySchoolConfirmationPost_Returns_Dashboard_Redirect_To_Action()
    {
        // Arrange
        ClaimsPrincipal user = new UserClaimsPrincipalFake().GetUserClaimsPrincipal();
        ControllerContext context = new()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };
        AdminController controller = GetAdminController();
        controller.ControllerContext = context;

        SecurityReportsBySchoolViewModel model = new()
        {
            SelectedConfirmationOption = "Admin"
        };

        // Act
        IActionResult result = controller.SecurityReportsBySchoolConfirmation(model);

        // Assert
        RedirectToActionResult viewResult = Assert.IsType<RedirectToActionResult>(result, exactMatch: false);
        Assert.Equal("Admin", viewResult.ControllerName);
        Assert.Equal("Index", viewResult.ActionName);
    }

    [Fact]
    public void AdminController_SecurityReportsBySchoolConfirmationPost_Returns_Validation_Message_If_No_Option_Selected()
    {
        // Arrange
        ClaimsPrincipal user = new UserClaimsPrincipalFake().GetUserClaimsPrincipal();
        ControllerContext context = new()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };
        AdminController controller = GetAdminController();
        controller.ControllerContext = context;

        SecurityReportsBySchoolViewModel model = new()
        {
            SelectedConfirmationOption = ""
        };

        // Act
        IActionResult result = controller.SecurityReportsBySchoolConfirmation(model);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        SecurityReportsBySchoolViewModel viewModel = viewResult.Model as SecurityReportsBySchoolViewModel;
        Assert.NotNull(viewResult);
        Assert.NotNull(viewModel);
        Assert.Equal("../Admin/SecurityReports/SecurityReportsBySchoolConfirmation", viewResult.ViewName);
        Assert.Single(controller.ViewData.ModelState["NoConfirmationSelection"].Errors);
    }

    [Fact]
    public async Task AdminController_SecurityReportsForYourOrganisation_DetailedSearches_Post_Returns_Correct_Data()
    {
        // Arrange
        ClaimsPrincipal user = _userClaimsPrincipalFake.GetUserClaimsPrincipal();
        Mock<ISession> mockSession = new();
        mockSession.Setup(x => x.Id).Returns("0");
        ControllerContext context = new()
        {
            HttpContext = new DefaultHttpContext() { User = user, Session = mockSession.Object }
        };

        Mock<IOptions<AzureAppSettings>> mockAzureAppSettings = new();
        mockAzureAppSettings.Setup(x => x.Value)
            .Returns(new AzureAppSettings() { IsSessionIdStoredInCookie = false });

        ReturnFile expected = new()
        {
            Bytes = new byte[200],
            FileName = "SecurityReport_DetailedSearches",
            FileType = "csv"
        };
        _downloadSecurityReportDetailedSearchesService.GetSecurityReportDetailedSearches(Arg.Any<string>(),
                Arg.Any<SecurityReportSearchType>(), Arg.Any<AzureFunctionHeaderDetails>(), Arg.Any<bool>())
            .Returns(expected);

        AdminController controller = new(
            _securityService,
            _sessionProvider,
            _downloadSecurityReportByUpnService,
            _downloadSecurityReportLoginDetailsService,
            _downloadSecurityReportDetailedSearchesService)
        {
            ControllerContext = context
        };

        SecurityReportsForYourOrganisationModel model = new();

        // Act
        IActionResult result = await controller.SecurityReportsForYourOrganisation(model);

        // Assert
        FileContentResult fileContentResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal(expected.Bytes.Length, fileContentResult.FileContents.Length);
    }

    [Fact]
    public async Task AdminController_SecurityReportsForYourOrganisation_LoginDetails_Post_Returns_Correct_Data()
    {
        // Arrange
        ClaimsPrincipal user = _userClaimsPrincipalFake.GetUserClaimsPrincipal();
        Mock<ISession> mockSession = new();
        mockSession.Setup(x => x.Id).Returns("1");
        ControllerContext context = new()
        {
            HttpContext = new DefaultHttpContext() { User = user, Session = mockSession.Object }
        };

        Mock<IOptions<AzureAppSettings>> mockAzureAppSettings = new();
        mockAzureAppSettings.Setup(x => x.Value)
            .Returns(new AzureAppSettings() { IsSessionIdStoredInCookie = false });

        ReturnFile expected = new()
        {
            Bytes = new byte[200],
            FileName = "SecurityReport_LoginDetails",
            FileType = "csv"
        };
        _downloadSecurityReportLoginDetailsService.GetSecurityReportLoginDetails(Arg.Any<string>(),
                Arg.Any<SecurityReportSearchType>(), Arg.Any<AzureFunctionHeaderDetails>())
            .Returns(expected);

        AdminController controller = new(
             _securityService,
             _sessionProvider,
             _downloadSecurityReportByUpnService,
             _downloadSecurityReportLoginDetailsService,
             _downloadSecurityReportDetailedSearchesService)
        {
            ControllerContext = context
        };

        SecurityReportsForYourOrganisationModel model = new()
        {
            DocumentId = "1"
        };

        // Act
        FileContentResult result = await controller.SecurityReportsForYourOrganisation(model) as FileContentResult;

        // Assert
        Assert.IsType<FileContentResult>(result);
        Assert.Equal(expected.Bytes.Length, result.FileContents.Length);
    }

    [Fact]
    public async Task AdminController_SecurityReportsForYourOrganisation_DetailedSearches_Post_Returns_Validation_Message_If_DocumentID_Is_Null()
    {
        // Arrange
        ClaimsPrincipal user = _userClaimsPrincipalFake.GetUserClaimsPrincipal();
        Mock<ISession> mockSession = new();
        mockSession.Setup(x => x.Id).Returns("12345");

        ControllerContext context = new()
        {
            HttpContext = new DefaultHttpContext() { User = user, Session = mockSession.Object }
        };

        Mock<IOptions<AzureAppSettings>> mockAzureAppSettings = new();
        mockAzureAppSettings.Setup(x => x.Value)
            .Returns(new AzureAppSettings() { IsSessionIdStoredInCookie = false });

        ReturnFile expected = new()
        {
            Bytes = new byte[200],
            FileName = "SecurityReport_DetailedSearches",
            FileType = "csv"
        };
        _downloadSecurityReportDetailedSearchesService.GetSecurityReportDetailedSearches(Arg.Any<string>(),
                Arg.Any<SecurityReportSearchType>(), Arg.Any<AzureFunctionHeaderDetails>(), Arg.Any<bool>())
            .Returns(expected);

        AdminController controller = GetAdminController();
        controller.ControllerContext = context;

        SecurityReportsForYourOrganisationModel model = new()
        {
            DocumentId = null
        };

        // Act
        IActionResult result = await controller.SecurityReportsForYourOrganisation(model);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        SecurityReportsForYourOrganisationModel viewModel = viewResult.Model as SecurityReportsForYourOrganisationModel;
        Assert.NotNull(viewModel);
        Assert.Equal("../Admin/SecurityReports/SecurityReportsForYourOrganisation", viewResult.ViewName);
        Assert.Single(controller.ViewData.ModelState["NoOrganisationalReportSelected"].Errors);
    }

    [Fact]
    public async Task AdminController_SecurityReportsForYourOrganisation_DetailedSearches_Post_Returns_Validation_Message_If_DocumentID_Is_Invalid()
    {
        // Arrange
        ClaimsPrincipal user = _userClaimsPrincipalFake.GetUserClaimsPrincipal();
        Mock<ISession> mockSession = new();
        mockSession.Setup(x => x.Id).Returns("12345");
        ControllerContext context = new()
        {
            HttpContext = new DefaultHttpContext() { User = user, Session = mockSession.Object }
        };

        Mock<IOptions<AzureAppSettings>> mockAzureAppSettings = new();
        mockAzureAppSettings.Setup(x => x.Value)
            .Returns(new AzureAppSettings() { IsSessionIdStoredInCookie = false });

        ReturnFile expected = new()
        {
            Bytes = new byte[200],
            FileName = "SecurityReport_DetailedSearches",
            FileType = "csv"
        };
        _downloadSecurityReportDetailedSearchesService.GetSecurityReportDetailedSearches(Arg.Any<string>(),
                Arg.Any<SecurityReportSearchType>(), Arg.Any<AzureFunctionHeaderDetails>(), Arg.Any<bool>())
            .Returns(expected);

        AdminController controller = GetAdminController();
        controller.ControllerContext = context;

        SecurityReportsForYourOrganisationModel model = new();

        // Act
        ViewResult result = await controller.SecurityReportsForYourOrganisation(model) as ViewResult;

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        SecurityReportsForYourOrganisationModel viewModel = viewResult.Model as SecurityReportsForYourOrganisationModel;
        Assert.NotNull(viewModel);
        Assert.Equal("../Admin/SecurityReports/SecurityReportsForYourOrganisation", viewResult.ViewName);
        Assert.Single(controller.ViewData.ModelState["NoDataForOrganisationalDownloadExists"].Errors);
    }
}
