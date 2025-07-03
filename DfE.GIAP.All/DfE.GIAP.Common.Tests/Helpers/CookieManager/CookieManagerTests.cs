using Microsoft.AspNetCore.Http;
using NSubstitute;
using Xunit;

namespace DfE.GIAP.Common.Tests.Helpers.CookieManager
{
    public class CookieManagerTests
    {
        private IRequestCookieCollection _cookieCollection;
        private IResponseCookies _responseCookies;

        [Fact]
        public void Get_returns_null_when_key_does_not_exist()
        {
            // Arrange
            var sut = new DfE.GIAP.Common.Helpers.CookieManager.CookieManager(SetupContextAccessor());

            // Act
            var result = sut.Get("null");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Get_returns_correctly_when_key_exists()
        {
            // Arrange
            var sut = new DfE.GIAP.Common.Helpers.CookieManager.CookieManager(SetupContextAccessor());

            // Act
            var result = sut.Get("test");

            // Assert
            Assert.Equal("string", result);
        }

        [Fact]
        public void Contains_throws_exception_when_httpcontext_is_null()
        {
            // Arrange
            var nullContext = Substitute.For<IHttpContextAccessor>();
            nullContext.HttpContext.Returns(x => null);
            var sut = new DfE.GIAP.Common.Helpers.CookieManager.CookieManager(nullContext);

            // Act
            // Assert
            Assert.Throws<ArgumentNullException>(() => sut.Contains("null"));
        }

        [Fact]
        public void Contains_throws_exception_when_key_is_null()
        {
            // Arrange
            var sut = new DfE.GIAP.Common.Helpers.CookieManager.CookieManager(SetupContextAccessor());

            // Act
            // Assert
            Assert.Throws<ArgumentNullException>(() => sut.Contains(null));
            Assert.Throws<ArgumentNullException>(() => sut.Contains(string.Empty));
        }

        [Fact]
        public void Contains_returns_true_when_key_exists()
        {
            // Arrange
            var sut = new DfE.GIAP.Common.Helpers.CookieManager.CookieManager(SetupContextAccessor());

            // Act
            var result = sut.Contains("test");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Delete_calls_cookiecollection_delete()
        {
            // Arrange
            var sut = new DfE.GIAP.Common.Helpers.CookieManager.CookieManager(SetupContextAccessor());

            // Act
            sut.Delete("test");

            // Assert
            _responseCookies.Received().Delete(Arg.Is<string>("test"));
        }

        [Fact]
        public void Set_throws_exception_if_key_is_null()
        {
            // Arrange
            var sut = new DfE.GIAP.Common.Helpers.CookieManager.CookieManager(SetupContextAccessor());

            // Act
            // Assert
            Assert.Throws<ArgumentNullException>(() => sut.Set(string.Empty, "value"));
        }

        [Fact]
        public void Set_throws_exception_if_value_is_null()
        {
            // Arrange
            var sut = new DfE.GIAP.Common.Helpers.CookieManager.CookieManager(SetupContextAccessor());

            // Act
            // Assert
            Assert.Throws<ArgumentNullException>(() => sut.Set("key", string.Empty));
        }

        [Fact]
        public void Set_appends_cookie_correctly_with_valid_values()
        {
            // Arrange
            var sut = new DfE.GIAP.Common.Helpers.CookieManager.CookieManager(SetupContextAccessor());

            // Act
            sut.Set("key", "value", true, 1, null);

            // Assert
            _responseCookies.Received().Append(
                Arg.Is<string>("key"),
                Arg.Is<string>("value"),
                Arg.Is<CookieOptions>(co => co.Secure && co.IsEssential)
            );
        }

        private IHttpContextAccessor SetupContextAccessor()
        {
            _cookieCollection = Substitute.For<IRequestCookieCollection>();
            _responseCookies = Substitute.For<IResponseCookies>();

            var request = Substitute.For<HttpRequest>();
            var response = Substitute.For<HttpResponse>();

            _cookieCollection["test"].Returns("string");
            _cookieCollection["null"].Returns(x => null);
            _cookieCollection.ContainsKey(Arg.Any<string>()).Returns(true);
            request.Cookies.Returns(_cookieCollection);

            response.Cookies.Returns(_responseCookies);

            var context = Substitute.For<HttpContext>();
            context.Request.Returns(request);
            context.Response.Returns(response);
            var contextAccessor = Substitute.For<IHttpContextAccessor>();
            contextAccessor.HttpContext.Returns(context);

            return contextAccessor;
        }
    }
}
