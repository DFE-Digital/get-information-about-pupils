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
        var context = CreateContext(true);
        var sessionProvider = Substitute.For<ISessionProvider>();
        sessionProvider.GetSessionValue(SessionKeys.ConsentKey).Returns((string)null);
        var middleware = CreateMiddleware(sessionProvider);

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
        var context = CreateContext(true);
        var sessionProvider = Substitute.For<ISessionProvider>();
        sessionProvider.GetSessionValue(SessionKeys.ConsentKey).Returns(SessionKeys.ConsentValue);
        var middleware = CreateMiddleware(sessionProvider);

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
        var context = CreateContext(true);

        var endpointMetadataCollection = new EndpointMetadataCollection(
                new AllowWithoutConsentAttribute()
            );
        var endpoint = new Endpoint(null, endpointMetadataCollection, "test");
        var endpointFeature = Substitute.For<IEndpointFeature>();
        endpointFeature.Endpoint.Returns(endpoint);
        context.Features.Set<IEndpointFeature>(endpointFeature);

        var sessionProvider = Substitute.For<ISessionProvider>();
        var middleware = CreateMiddleware(sessionProvider);

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
        var context = CreateContext(false);
        var sessionProvider = Substitute.For<ISessionProvider>();
        var middleware = CreateMiddleware(sessionProvider);

        // Act
        await middleware.InvokeAsync(context, sessionProvider);

        // Assert
        Assert.NotEqual(StatusCodes.Status302Found, context.Response.StatusCode);
        Assert.DoesNotContain(Routes.Application.Consent, context.Response.Headers["location"].ToString());
    }

    private HttpContext CreateContext(bool isAuthenticated)
    {
        var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[0], isAuthenticated ? "fake" : null));
        HttpContext context = new DefaultHttpContext();
        context.Session = new TestSession();
        context.User = userPrincipal;
        return context;
    }

    private ConsentRedirectMiddleware CreateMiddleware(ISessionProvider sessionProvider)
    {
        var requestDelegate = new RequestDelegate(
            (innerContext) => Task.CompletedTask);
        return new ConsentRedirectMiddleware(requestDelegate);
    }
}
