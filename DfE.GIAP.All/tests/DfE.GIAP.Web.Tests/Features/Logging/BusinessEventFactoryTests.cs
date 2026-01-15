using DfE.GIAP.Core.Common.CrossCutting.Logging.Events;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Events.Models;
using DfE.GIAP.Web.Features.Logging;
using DfE.GIAP.Web.Tests.Shared.Http;
using DfE.GIAP.Web.Tests.TestDoubles;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.Logging;

public class BusinessEventFactoryTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly BusinessEventFactory _sut;

    public BusinessEventFactoryTests()
    {
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _sut = new BusinessEventFactory(_httpContextAccessorMock.Object);
    }

    [Fact]
    public void Constructor_WhenHttpContextAccessorIsNull_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new BusinessEventFactory(null!));
    }

    [Fact]
    public void Constructor_WhenHttpContextAccessorIsProvided_ShouldNotThrow()
    {
        // Arrange
        Mock<IHttpContextAccessor> httpContextAccessorMock = new();

        // Act
        BusinessEventFactory sut = new(httpContextAccessorMock.Object);

        // Assert
        Assert.NotNull(sut);
    }

    [Fact]
    public void CreateSearch_Returns_Model_With_PassedInData()
    {
        // Arrange
        HttpContext httpContext = HttpContextTestDoubles.WithUser(UserClaimsPrincipalFake.GetLAUserClaimsPrincipal());
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        Dictionary<string, bool> filterFlags = new() { { "flag1", true } };

        // Act
        SearchEvent result = _sut.CreateSearch(SearchIdentifierType.UPN, true, filterFlags);

        Assert.Equal(SearchIdentifierType.UPN, result.Payload.SearchIdentifierType);
        Assert.True(result.Payload.IsCustomSearch);
        Assert.Equal(filterFlags, result.Payload.FilterFlags);
    }

    [Fact]
    public void CreateDownload_Returns_Model_With_PassedInData()
    {
        // Arrange
        HttpContext httpContext = HttpContextTestDoubles.WithUser(UserClaimsPrincipalFake.GetLAUserClaimsPrincipal());
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        Dataset dataset = Dataset.KS1;
        string batchId = Guid.NewGuid().ToString();

        // Act
        DownloadEvent result = _sut.CreateDownload(
            DownloadType.Search,
            DownloadFileFormat.CSV,
            DownloadEventType.NPD,
            batchId,
            dataset);

        Assert.Equal(DownloadType.Search, result.Payload.DownloadType);
        Assert.Equal(DownloadFileFormat.CSV, result.Payload.DownloadFileFormat);
        Assert.Equal(DownloadEventType.NPD, result.Payload.DownloadEventType);
        Assert.Equal(batchId, result.Payload.BatchId);
        Assert.Equal(dataset, result.Payload.Dataset);
    }

    [Fact]
    public void CreateSignin_Returns_Model_With_PassedInData()
    {
        // Arrange
        var userId = "signin-user";
        var sessionId = "signin-session";
        var urn = "signin-urn";
        var orgName = "Signin Org";
        var orgCategory = "Gov";

        // Act
        var result = _sut.CreateSignin(userId, sessionId, urn, orgName, orgCategory);

        // Assert: only check that passed-in data is attached
        Assert.Equal(userId, result.UserId);
        Assert.Equal(sessionId, result.SessionId);
        Assert.Equal(urn, result.OrgURN);
        Assert.Equal(orgName, result.OrgName);
        Assert.Equal(orgCategory, result.OrgCategory);
    }
}
