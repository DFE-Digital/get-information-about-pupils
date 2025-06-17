using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Helpers.Consent;
using DfE.GIAP.Web.Middleware;
using DfE.GIAP.Web.Tests.FakeData;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using NSubstitute;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace DfE.GIAP.Web.Tests.Middleware;

public class ConsentRedirectMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_redirects_when_consent_not_given()
    {
        // Arrange
        var context = CreateContext(true);
        var middleware = CreateMiddleware();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(context.Response.StatusCode == StatusCodes.Status302Found);
        Assert.True(context.Response.Headers["location"].Equals(Routes.Application.Consent));
    }

    [Fact]
    public async Task InvokeAsync_does_not_redirect_when_consent_given()
    {
        // Arrange
        var context = CreateContext(true);
        var middleware = CreateMiddleware();
        ConsentHelper.SetConsent(context);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.False(context.Response.StatusCode == StatusCodes.Status302Found);
        Assert.False(context.Response.Headers["location"].Equals("/Home"));
    }

    [Fact]
    public async Task InvokeAsync_does_not_redirect_when_attribute_present()
    {
        // Arrange
        var context = CreateContext(true);

        var endpointMetadataCollection = new EndpointMetadataCollection(
                new AllowWithoutConsentAttribute()
            );
        var endpoint = new Endpoint(null, endpointMetadataCollection, "test");
        var endpointFeature = Substitute.For<IEndpointFeature>();
        endpointFeature.Endpoint.Returns(endpoint);
        context.Features.Set<IEndpointFeature>(endpointFeature);

        var middleware = CreateMiddleware();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.False(context.Response.StatusCode == StatusCodes.Status302Found);
        Assert.False(context.Response.Headers["location"].Equals("/Home"));
    }

    [Fact]
    public async Task InvokeAsync_does_not_redirect_when_user_not_logged_in()
    {
        // Arrange
        var context = CreateContext(false);
        var middleware = CreateMiddleware();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.False(context.Response.StatusCode == StatusCodes.Status302Found);
        Assert.False(context.Response.Headers["location"].Equals("/Home"));
    }

    private HttpContext CreateContext(bool isAuthenticated)
    {
        var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[0], "fake"));
        HttpContext context = new DefaultHttpContext();
        context.Session = new TestSession();

        if (isAuthenticated)
        {
            context.User = userPrincipal;
        }

        return context;
    }

    private ConsentRedirectMiddleware CreateMiddleware()
    {
        var requestDelegate = new RequestDelegate(
            (innerContext) => Task.FromResult(0));
        return new ConsentRedirectMiddleware(requestDelegate);
    }
}
