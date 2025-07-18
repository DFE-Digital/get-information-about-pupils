using DfE.GIAP.Web.Authorisation;
using DfE.GIAP.Web.Tests.TestDoubles;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Authorisation;

public sealed class HttpContextAuthorisationContextTests
{

    [Fact]
    public void Constructor_WithNullAccessor_ThrowsArgumentNullException()
    {
        // Act Assert
        Assert.Throws<ArgumentNullException>(() => new HttpContextAuthorisationContext(null));
    }

    [Fact]
    public void Constructor_WithNullHttpContext_ThrowsArgumentNullException()
    {
        // Arrange
        Mock<IHttpContextAccessor> accessor = new();
        accessor.Setup(a => a.HttpContext).Returns((HttpContext)null!);

        // Act Assert
        Assert.Throws<ArgumentNullException>(() => new HttpContextAuthorisationContext(accessor.Object));
    }

    [Fact]
    public void Constructor_WithValidClaims_InitialisesPropertiesCorrectly()
    {
        // Arrange
        DefaultHttpContext context = HttpContextBuilder.Create()
            .WithUserId("user-123")
            .WithOrganisationAgeRange(5, 16)
            .AsAdmin()
            .Build();

        Mock<IHttpContextAccessor> accessor = new();
        accessor.Setup(a => a.HttpContext).Returns(context);

        // Act
        HttpContextAuthorisationContext result = new(accessor.Object);

        // Assert
        Assert.Equal("user-123", result.UserId);
        Assert.Equal(5, result.LowAge);
        Assert.Equal(16, result.HighAge);
        Assert.True(result.IsAdministrator);
    }

    [Fact]
    public void Constructor_WithMissingClaims_UsesDefaults()
    {
        // Arrange
        DefaultHttpContext context = HttpContextBuilder.Create().Build();

        Mock<IHttpContextAccessor> accessor = new();
        accessor.Setup(a => a.HttpContext).Returns(context);

        // Act
        HttpContextAuthorisationContext result = new(accessor.Object);

        // Assert
        Assert.Null(result.UserId);
        Assert.Equal(0, result.LowAge);
        Assert.Equal(0, result.HighAge);
        Assert.False(result.IsAdministrator);
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

        Mock<IHttpContextAccessor> accessor = new();
        accessor.Setup(a => a.HttpContext).Returns(context);

        // Act
        HttpContextAuthorisationContext result = await Task.Run(() => new HttpContextAuthorisationContext(accessor.Object));

        // Assert
        Assert.Equal("async", result.UserId);
        Assert.Equal(10, result.LowAge);
        Assert.Equal(18, result.HighAge);
        Assert.True(result.IsAdministrator);
    }

    [Fact]
    public async Task Constructor_InsideConfigureAwaitFalseContext_DoesNotThrow()
    {
        // Arrange
        DefaultHttpContext context = HttpContextBuilder.Create()
            .WithUserId("async-no-return-to-scheduling-context")
            .WithOrganisationAgeRange(11, 17)
            .AsAdmin()
            .Build();

        Mock<IHttpContextAccessor> accessor = new();
        accessor.Setup(a => a.HttpContext).Returns(context);

        HttpContextAuthorisationContext? result = null;

        // Act
#pragma warning disable xUnit1030 // Do not call ConfigureAwait(false) in test method
        await Task.Run(() =>
        {
            result = new HttpContextAuthorisationContext(accessor.Object);
        }).ConfigureAwait(false);
#pragma warning restore xUnit1030 // Do not call ConfigureAwait(false) in test method

        // Assert
        Assert.Equal("async-no-return-to-scheduling-context", result.UserId);
        Assert.Equal(11, result.LowAge);
        Assert.Equal(17, result.HighAge);
        Assert.True(result.IsAdministrator);
    }
}
