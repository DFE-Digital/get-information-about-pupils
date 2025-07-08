using DfE.GIAP.Service.Common;
using DfE.GIAP.Web.Helpers.Banner;
using DfE.GIAP.Web.Tests.TestDoubles;
using Microsoft.AspNetCore.Http;
using Moq;
using NSubstitute;
using System.Threading.Tasks;
using Xunit;

namespace DfE.GIAP.Web.Tests.Helpers
{
    public class BannerHelperTests
    {
        private readonly Mock<ICommonService> _commonService = new Mock<ICommonService>();
        private readonly IHttpContextAccessor _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        private readonly TestSession _mockSession = new TestSession();

        [Fact]
        public async Task SetLatestNewsStatus_set_showNewsBanner_When_GetLatestNewsStatus_is_true()
        {
            // Arrange
            var sut = GetLatestNewsBanner();

            // Act
             _commonService.Setup(x => x.GetLatestNewsStatus(It.IsAny<string>())).ReturnsAsync(true);
            await sut.SetLatestNewsStatus();

            // Assert
            Assert.NotNull(_httpContextAccessor?.HttpContext.Session.GetString("showNewsBanner"));
        }

        [Fact]
        public async Task SetLatestNewsStatus_doesNot_set_showNewsBanner_When_GetLatestNewsStatus_is_false()
        {
            // Arrange
            var sut = GetLatestNewsBanner();

            // Act
            _commonService.Setup(x => x.GetLatestNewsStatus(It.IsAny<string>())).ReturnsAsync(false);
            await sut.SetLatestNewsStatus();

            // Assert
            Assert.Empty(_httpContextAccessor?.HttpContext.Session.GetString("showNewsBanner"));
        }

        [Fact]
        public async Task RemoveLatestNewsStatus_Removes_showNewsBanner_In_Session()
        {
            // Arrange
            var sut = GetLatestNewsBanner();
            _commonService.Setup(x => x.GetLatestNewsStatus(It.IsAny<string>())).ReturnsAsync(true);
            await sut.SetLatestNewsStatus();

            // Act
            await sut.RemoveLatestNewsStatus();

            // Assert
            Assert.Empty(_httpContextAccessor?.HttpContext.Session.GetString("showNewsBanner"));
        }

        [Fact]
        public async Task ShowNewsBanner_returns_false_if_called_from_Consent()
        {
            // Arrange
            var sut = GetLatestNewsBanner();
            _commonService.Setup(x => x.GetLatestNewsStatus(It.IsAny<string>())).ReturnsAsync(true);
            await sut.SetLatestNewsStatus();
            _httpContextAccessor.HttpContext.Request.Path = "/";

            // Act
            var result = sut.ShowNewsBanner();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ShowNewsBanner_returns_false_if_showNewsBanner_Not_In_Session()
        {
            // Arrange
            var sut = GetLatestNewsBanner();
            _commonService.Setup(x => x.GetLatestNewsStatus(It.IsAny<string>())).ReturnsAsync(false);
            await sut.SetLatestNewsStatus();

            // Act
            var result = sut.ShowNewsBanner();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task  ShowNewsBanner_returns_true_if_showNewsBannert_In_Session()
        {
            // Arrange
            var sut = GetLatestNewsBanner();
            _commonService.Setup(x => x.GetLatestNewsStatus(It.IsAny<string>())).ReturnsAsync(true);
            await sut.SetLatestNewsStatus();

            // Act
            var result = sut.ShowNewsBanner();

            // Assert
            Assert.True(result);
        }

        public LatestNewsBanner GetLatestNewsBanner()
        {
            var user = new UserClaimsPrincipalFake().GetUserClaimsPrincipal();

            var httpContext = new DefaultHttpContext() { User = user, Session = _mockSession };
            _httpContextAccessor.HttpContext.Returns(httpContext);

            return new LatestNewsBanner(_commonService.Object, _httpContextAccessor);
        }
    }
}
