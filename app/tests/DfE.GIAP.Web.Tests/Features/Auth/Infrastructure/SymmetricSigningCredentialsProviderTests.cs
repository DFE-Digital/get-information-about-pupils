using System.Text;
using DfE.GIAP.Web.Features.Auth.Infrastructure;
using DfE.GIAP.Web.Features.Auth.Infrastructure.Config;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.Auth.Infrastructure;

public class SymmetricSigningCredentialsProviderTests
{
    [Fact]
    public void Constructor_ThrowArgumentNullException_When_NullOptionsValue()
    {
        // Arrange
        IOptions<DsiOptions> options = Options.Create<DsiOptions>(null!);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new SymmetricSigningCredentialsProvider(options));
    }

    [Fact]
    public void GetSigningCredentials_WithValidSecret_ShouldReturnSigningCredentials()
    {
        // Arrange
        string secret = "valid-secret";
        IOptions<DsiOptions> options = Options.Create(new DsiOptions { ApiClientSecret = secret });
        SymmetricSigningCredentialsProvider sut = new(options);

        // Act
        SigningCredentials credentials = sut.GetSigningCredentials();

        // Assert
        Assert.NotNull(credentials);
        Assert.IsType<SigningCredentials>(credentials);
        Assert.Equal(SecurityAlgorithms.HmacSha256, credentials.Algorithm);

        SymmetricSecurityKey expectedKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));
        Assert.Equal(expectedKey.Key, ((SymmetricSecurityKey)credentials.Key).Key);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GetSigningCredentials_WithMissingSecret_ShouldThrowSecurityTokenSignatureKeyNotFoundException(string? secret)
    {
        // Arrange
        IOptions<DsiOptions> options = Options.Create(new DsiOptions { ApiClientSecret = secret });
        SymmetricSigningCredentialsProvider sut = new(options);

        // Act & Assert
        Assert.Throws<SecurityTokenSignatureKeyNotFoundException>(() => sut.GetSigningCredentials());
    }
}
