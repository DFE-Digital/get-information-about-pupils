using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Middleware;
using DfE.GIAP.Web.Providers.Session;
using DfE.GIAP.Web.Tests.TestDoubles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using NSubstitute;
using System.Security.Claims;
using Xunit;

namespace DfE.GIAP.Web.Tests.Middleware;

public class ConsentRedirectMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_redirects_when_consent_not_given()
    {
        // Arrange
        HttpContext context = CreateContext(true);
        ISessionProvider sessionProvider = Substitute.For<ISessionProvider>();
        sessionProvider.GetSessionValue(SessionKeys.ConsentGivenKey).Returns((string)null);
        ConsentRedirectMiddleware middleware = CreateMiddleware();

        // Act
        await middleware.InvokeAsync(context, sessionProvider);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, context.Response.StatusCode);
        Assert.Equal(Routes.Application.Consent, context.Response.Headers["location"]);
    }

    [Fact]
    public async Task InvokeAsync_does_not_redirect_when_consent_given()
    {
        // Arrange
        HttpContext context = CreateContext(true);
        ISessionProvider sessionProvider = Substitute.For<ISessionProvider>();
        sessionProvider.GetSessionValueOrDefault<bool>(SessionKeys.ConsentGivenKey).Returns(true);
        ConsentRedirectMiddleware middleware = CreateMiddleware();

        // Act
        await middleware.InvokeAsync(context, sessionProvider);

        // Assert
        Assert.NotEqual(StatusCodes.Status302Found, context.Response.StatusCode);
        Assert.DoesNotContain(Routes.Application.Consent, context.Response.Headers["location"].ToString());
    }

    [Fact]
    public async Task InvokeAsync_does_not_redirect_when_attribute_present()
    {
        // Arrange
        HttpContext context = CreateContext(true);

        EndpointMetadataCollection endpointMetadataCollection = new(
                new AllowWithoutConsentAttribute());
        Endpoint endpoint = new Endpoint(null, endpointMetadataCollection, "test");
        IEndpointFeature endpointFeature = Substitute.For<IEndpointFeature>();
        endpointFeature.Endpoint.Returns(endpoint);
        context.Features.Set(endpointFeature);

        ISessionProvider sessionProvider = Substitute.For<ISessionProvider>();
        ConsentRedirectMiddleware middleware = CreateMiddleware();

        // Act
        await middleware.InvokeAsync(context, sessionProvider);

        // Assert
        Assert.NotEqual(StatusCodes.Status302Found, context.Response.StatusCode);
        Assert.DoesNotContain(Routes.Application.Consent, context.Response.Headers["location"].ToString());
    }

    [Fact]
    public async Task InvokeAsync_does_not_redirect_when_user_not_logged_in()
    {
        // Arrange
        HttpContext context = CreateContext(false);
        ISessionProvider sessionProvider = Substitute.For<ISessionProvider>();
        ConsentRedirectMiddleware middleware = CreateMiddleware();

        // Act
        await middleware.InvokeAsync(context, sessionProvider);

        // Assert
        Assert.NotEqual(StatusCodes.Status302Found, context.Response.StatusCode);
        Assert.DoesNotContain(Routes.Application.Consent, context.Response.Headers["location"].ToString());
    }

    private HttpContext CreateContext(bool isAuthenticated)
    {
        ClaimsPrincipal userPrincipal = new(new ClaimsIdentity(new Claim[0], isAuthenticated ? "fake" : null));
        HttpContext context = new DefaultHttpContext();
        context.Session = new TestSession();
        context.User = userPrincipal;
        return context;
    }

    private static ConsentRedirectMiddleware CreateMiddleware()
    {
        RequestDelegate requestDelegate = new(
            (innerContext) => Task.CompletedTask);
        return new ConsentRedirectMiddleware(requestDelegate);
    }
}
