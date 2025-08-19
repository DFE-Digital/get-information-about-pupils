using System.Security.Claims;
using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Service.Common;
using DfE.GIAP.Service.PreparedDownloads;
using DfE.GIAP.Web.Controllers.PreparedDownload;
using DfE.GIAP.Web.Tests.TestDoubles;
using DfE.GIAP.Web.ViewModels.PrePreparedDownload;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using NSubstitute;
using Xunit;

namespace DfE.GIAP.Web.Tests.Controllers.PrePreparedDownloads;

[Trait("PreparedDownloads", "PreparedDownloads Controller Unit Tests")]
public class PrePreparedDownloadsControllerTests : IClassFixture<PreparedDownloadsResultsFake>,
                                                IClassFixture<UserClaimsPrincipalFake>
{
    private readonly PreparedDownloadsResultsFake _preparedDownloadsResultsFake;
    private readonly UserClaimsPrincipalFake _userClaimsPrincipalFake;
    private readonly IPrePreparedDownloadsService _mockprePreparedDownloadsService = Substitute.For<IPrePreparedDownloadsService>();
    private readonly ISession _mockSession = Substitute.For<ISession>();
    private readonly IOptions<AzureAppSettings> _mockAzureAppSettings = Substitute.For<IOptions<AzureAppSettings>>();

    public PrePreparedDownloadsControllerTests(PreparedDownloadsResultsFake preparedDownloadsResultsFake,
                                            UserClaimsPrincipalFake userClaimsPrincipalFake)
    {
        _preparedDownloadsResultsFake = preparedDownloadsResultsFake;
        _userClaimsPrincipalFake = userClaimsPrincipalFake;
    }

    [Fact]
    public async Task PreparedDownloadsController_Should_Returns_Correct_View()
    {
        // Arrange
        Mock<IPrePreparedDownloadsService> mockPrePreparedDownloads = new();

        mockPrePreparedDownloads.Setup((repo) =>
            repo.GetPrePreparedDownloadsList(
                It.IsAny<AzureFunctionHeaderDetails>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(_preparedDownloadsResultsFake.GetPrePreparedDownloadsList());

        _mockAzureAppSettings.Value.Returns(new AzureAppSettings
        {
            IsSessionIdStoredInCookie = true
        });

        PreparedDownloadsController controller = GetPreparedDownloadsController(mockPrePreparedDownloads);

        // Act
        IActionResult result = await controller.GetPreparedDownloadsAsync();

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result, exactMatch: false);
        Assert.NotNull(viewResult);

        PreparedDownloadsViewModel? viewModel = viewResult.Model as PreparedDownloadsViewModel;
        Assert.NotNull(viewModel);

        Assert.Equal("~/Views/PrePreparedDownloads/PrePreparedDownload.cshtml", viewResult.ViewName);
    }

    [Fact]
    public async Task PreparedDownloadsController_Should_Returns_List_With_PrePrepareddownloads()
    {
        // Arrange
        List<Domain.Models.PrePreparedDownloads.PrePreparedDownloads> results = _preparedDownloadsResultsFake.GetPrePreparedDownloadsList();
        Mock<IPrePreparedDownloadsService> mockPrePreparedDownloads = new();
        mockPrePreparedDownloads.Setup(repo => repo.GetPrePreparedDownloadsList(It.IsAny<AzureFunctionHeaderDetails>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(_preparedDownloadsResultsFake.GetPrePreparedDownloadsList());
        _mockAzureAppSettings.Value.Returns(new AzureAppSettings
        {
            IsSessionIdStoredInCookie = true
        });
        PreparedDownloadsController controller = GetPreparedDownloadsController(mockPrePreparedDownloads);

        // Act
        IActionResult result = await controller.GetPreparedDownloadsAsync();

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);

        PreparedDownloadsViewModel? viewModel = viewResult.Model as PreparedDownloadsViewModel;
        Assert.NotNull(viewModel);

        Assert.Equal(results.Count, viewModel.PreparedDownloadFiles.Count);

    }
    [Fact]
    public async Task GetPreparedDownloadsController_DownloadPrePreparedFile_Should_Return_File()
    {
        // Arrange
        FileStreamResult model = _preparedDownloadsResultsFake.GetMetaDataFile();
        Mock<IPrePreparedDownloadsService> mockPrePreparedDownloads = new();
        mockPrePreparedDownloads.Setup((repo) =>
            repo.GetPrePreparedDownloadsList(
                It.IsAny<AzureFunctionHeaderDetails>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(_preparedDownloadsResultsFake.GetPrePreparedDownloadsList());

        _mockAzureAppSettings.Value.Returns(new AzureAppSettings
        {
            IsSessionIdStoredInCookie = true
        });
        AzureFunctionHeaderDetails azureFunctionHeaderDetails = new () { ClientId = "123456", SessionId = "654321" };
        MemoryStream ms = new();
        _mockprePreparedDownloadsService.PrePreparedDownloadsFileAsync("", ms, azureFunctionHeaderDetails).Returns(Task.FromResult(model));
        _mockAzureAppSettings.Value.Returns(new AzureAppSettings { IsSessionIdStoredInCookie = false });
        PreparedDownloadsController controller = GetPreparedDownloadsController(mockPrePreparedDownloads);

        // Act
        FileStreamResult result = await controller.DownloadPrePreparedFile("PrePreparedDownloadTestFile.csv", DateTime.Now);

        // Assert
        Assert.Equal(result.FileDownloadName, model.FileDownloadName);
        Assert.Equal(result.ContentType, model.ContentType);
    }

    private PreparedDownloadsController GetPreparedDownloadsController(Mock<IPrePreparedDownloadsService> mockPrePreparedDownloadsService)
    {
        ClaimsPrincipal user = _userClaimsPrincipalFake.GetUserClaimsPrincipal();

        Mock<ICommonService> _commonService = new();

        return new PreparedDownloadsController(_commonService.Object, mockPrePreparedDownloadsService.Object)
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext { User = user, Session = _mockSession },
            }
        };
    }

}
