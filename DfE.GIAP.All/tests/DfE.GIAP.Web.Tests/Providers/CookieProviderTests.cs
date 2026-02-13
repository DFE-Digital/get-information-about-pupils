using DfE.GIAP.Web.Providers.Cookie;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Providers;

public sealed class CookieProviderTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<HttpContext> _httpContextMock;
    private readonly Mock<IRequestCookieCollection> _requestCookiesMock;
    private readonly Mock<IResponseCookies> _responseCookiesMock;
    private readonly CookieProvider _cookieProvider;

    public CookieProviderTests()
    {
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _httpContextMock = new Mock<HttpContext>();
        _requestCookiesMock = new Mock<IRequestCookieCollection>();
        _responseCookiesMock = new Mock<IResponseCookies>();

        _httpContextMock.SetupGet(x => x.Request.Cookies).Returns(_requestCookiesMock.Object);
        _httpContextMock.SetupGet(x => x.Response.Cookies).Returns(_responseCookiesMock.Object);
        _httpContextAccessorMock.SetupGet(x => x.HttpContext).Returns(_httpContextMock.Object);

        _cookieProvider = new CookieProvider(_httpContextAccessorMock.Object);
    }

    [Fact]
    public void Get_ReturnsCookieValue_WhenCookieExists()
    {
        string key = "test";
        string value = Uri.EscapeDataString("value");
        _requestCookiesMock.Setup(c => c[key]).Returns(value);

        string result = _cookieProvider.Get(key);

        Assert.Equal("value", result);
    }

    [Fact]
    public void Get_ReturnsNull_WhenCookieDoesNotExist()
    {
        string key = "missing";
        _requestCookiesMock.Setup(c => c[key]).Returns<string>(null!);

        string result = _cookieProvider.Get(key);

        Assert.Null(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Get_ThrowsArgumentNullException_WhenKeyIsNullOrEmpty(string? key)
    {
        Assert.Throws<ArgumentNullException>(() => _cookieProvider.Get(key));
    }

    [Fact]
    public void Contains_ReturnsTrue_WhenCookieExists()
    {
        string key = "exists";
        _requestCookiesMock.Setup(c => c.ContainsKey(key)).Returns(true);

        bool result = _cookieProvider.Contains(key);

        Assert.True(result);
    }

    [Fact]
    public void Contains_ReturnsFalse_WhenCookieDoesNotExist()
    {
        string key = "notfound";
        _requestCookiesMock.Setup(c => c.ContainsKey(key)).Returns(false);

        bool result = _cookieProvider.Contains(key);

        Assert.False(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Contains_ThrowsArgumentNullException_WhenKeyIsNullOrEmpty(string? key)
    {
        Assert.Throws<ArgumentNullException>(() => _cookieProvider.Contains(key));
    }

    [Fact]
    public void Delete_DeletesCookie()
    {
        string key = "delete";
        _responseCookiesMock.Setup(c => c.Delete(key));

        _cookieProvider.Delete(key);

        _responseCookiesMock.Verify(c => c.Delete(key), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Delete_ThrowsArgumentNullException_WhenKeyIsNullOrEmpty(string? key)
    {
        Assert.Throws<ArgumentNullException>(() => _cookieProvider.Delete(key));
    }

    [Fact]
    public void Set_AppendsCookie_WithDefaultOptions()
    {
        string key = "set";
        string value = "val";
        CookieOptions? capturedOptions = null;

        _responseCookiesMock
            .Setup(c => c.Append(key, value, It.IsAny<CookieOptions>()))
            .Callback<string, string, CookieOptions>((_, _, opt) => capturedOptions = opt);

        _cookieProvider.Set(key, value);

        _responseCookiesMock.Verify(c => c.Append(key, value, It.IsAny<CookieOptions>()), Times.Once);
        Assert.NotNull(capturedOptions);
        Assert.True(capturedOptions.Secure);
        Assert.True(capturedOptions.HttpOnly);
    }

    [Fact]
    public void Set_OverridesExpireTime_WhenExpireTimeIs1()
    {
        string key = "set";
        string value = "val";
        CookieOptions? capturedOptions = null;

        _responseCookiesMock
            .Setup(c => c.Append(key, value, It.IsAny<CookieOptions>()))
            .Callback<string, string, CookieOptions>((_, _, opt) => capturedOptions = opt);

        _cookieProvider.Set(key, value, expireTime: 1);

        Assert.NotNull(capturedOptions);
        Assert.True((capturedOptions.Expires! - DateTime.Now).Value.TotalMinutes <= 20.1);
    }

    [Theory]
    [InlineData(null, "value")]
    [InlineData("key", null)]
    [InlineData("", "value")]
    [InlineData("key", "")]
    public void Set_ThrowsArgumentNullException_WhenKeyOrValueIsNullOrEmpty(string? key, string? value)
    {
        Assert.Throws<ArgumentNullException>(() => _cookieProvider.Set(key, value));
    }

    [Fact]
    public void Set_SetsIsEssential_WhenSpecified()
    {
        string key = "essential";
        string value = "val";
        CookieOptions? capturedOptions = null;

        _responseCookiesMock
            .Setup(c => c.Append(key, value, It.IsAny<CookieOptions>()))
            .Callback<string, string, CookieOptions>((_, _, opt) => capturedOptions = opt);

        _cookieProvider.Set(key, value, isEssential: true);

        Assert.NotNull(capturedOptions);
        Assert.True(capturedOptions.IsEssential);
    }

    [Fact]
    public void ClearCookies_DeletesAllCookies()
    {
        List<string> keys = ["a", "b", "c"];
        _requestCookiesMock.Setup(c => c.Keys).Returns(keys);

        _responseCookiesMock.Setup(c => c.Delete(It.IsAny<string>()));

        _cookieProvider.ClearCookies();

        foreach (string key in keys)
        {
            _responseCookiesMock.Verify(c => c.Delete(key), Times.Once);
        }
    }
}
