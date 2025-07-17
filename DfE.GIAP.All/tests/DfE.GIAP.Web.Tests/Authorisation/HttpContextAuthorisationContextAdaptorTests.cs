using DfE.GIAP.Web.Authorisation;
using DfE.GIAP.Web.Tests.TestDoubles;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace DfE.GIAP.Web.Tests.Authorisation;


public sealed class HttpContextAuthorisationContextTests
{
    [Fact]
    public void Constructor_WithValidClaims_InitialisesPropertiesCorrectly()
    {
        // Arrange
        DefaultHttpContext context = HttpContextBuilder.Create()
            .WithUserId("user")
            .WithOrganisationAgeRange(5, 16)
            .AsAdmin()
            .Build();

        // Act
        HttpContextAuthorisationContext result = new(context);

        // Assert
        Assert.Equal("user", result.UserId);
        Assert.Equal(5, result.LowAge);
        Assert.Equal(16, result.HighAge);
        Assert.True(result.IsAdministrator);
    }

    [Fact]
    public void Constructor_WithMissingClaims_UsesDefaults()
    {
        // Arrange
        DefaultHttpContext context =
            HttpContextBuilder.Create()
                .Build();

        // Act
        HttpContextAuthorisationContext result = new(context);

        // Assert
        Assert.Equal(string.Empty, result.UserId);
        Assert.Equal(0, result.LowAge);
        Assert.Equal(0, result.HighAge);
        Assert.False(result.IsAdministrator);
    }

    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        // Arrange
        Func<HttpContextAuthorisationContext> act = () => new HttpContextAuthorisationContext(null!);

        // Act Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public async Task Constructor_InAsyncContext_DoesNotThrowOrMisbehave()
    {
        // Arrange
        DefaultHttpContext context = HttpContextBuilder.Create()
            .WithUserId("async")
            .WithOrganisationAgeRange(10, 18)
            .AsAdmin()
            .Build();

        // Act
        HttpContextAuthorisationContext result = await Task.Run(() =>
            new HttpContextAuthorisationContext(context));

        // Assert
        Assert.Equal("async", result.UserId);
        Assert.Equal(10, result.LowAge);
        Assert.Equal(18, result.HighAge);
        Assert.True(result.IsAdministrator);
    }

    [Fact]
    public async Task Constructor_InAsyncContext_WithConfigureAwaitFalseContext_DoesNotThrow()
    {
        // Arrange
        DefaultHttpContext context = HttpContextBuilder.Create()
            .WithUserId("async-drop-context")
            .WithOrganisationAgeRange(11, 17)
            .AsAdmin()
            .Build();

        HttpContextAuthorisationContext result = null;

        // Act
#pragma warning disable xUnit1030 // Do not call ConfigureAwait(false) in test method
        await Task.Run(() =>
        {
            result = new HttpContextAuthorisationContext(context);
        }).ConfigureAwait(false);
#pragma warning restore xUnit1030 // Do not call ConfigureAwait(false) in test method

        // Assert
        Assert.Equal("async-drop-context", result.UserId);
        Assert.Equal(11, result.LowAge);
        Assert.Equal(17, result.HighAge);
        Assert.True(result.IsAdministrator);
    }
}
