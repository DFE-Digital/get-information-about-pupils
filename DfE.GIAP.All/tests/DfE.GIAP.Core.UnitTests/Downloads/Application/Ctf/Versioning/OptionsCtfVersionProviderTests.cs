using DfE.GIAP.Core.Downloads.Application.Ctf.Options;
using DfE.GIAP.Core.Downloads.Application.Ctf.Versioning;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Core.UnitTests.Downloads.Application.Ctf.Versioning;

public class OptionsCtfVersionProviderTests
{
    [Fact]
    public void Constructor_WithValidVersion_Succeeds()
    {
        CtfOptions options = new()
        {
            Version = "1.2.3"
        };

        IOptions<CtfOptions> wrappedOptions = Options.Create(options);
        OptionsCtfVersionProvider provider = new(wrappedOptions);

        Assert.NotNull(provider);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithWhitespaceVersion_Throws(string version)
    {
        CtfOptions options = new()
        {
            Version = version!
        };

        IOptions<CtfOptions> wrappedOptions = Options.Create(options);
        Action act = new(() => new OptionsCtfVersionProvider(wrappedOptions));

        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Constructor_WithNullVersion_Throws()
    {
        CtfOptions options = new()
        {
            Version = null!
        };

        IOptions<CtfOptions> wrappedOptions = Options.Create(options);
        Action act = new(() => new OptionsCtfVersionProvider(wrappedOptions));

        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void GetVersion_ReturnsConfiguredVersion()
    {
        CtfOptions options = new()
        {
            Version = "1.2.3"
        };

        IOptions<CtfOptions> wrappedOptions = Options.Create(options);
        OptionsCtfVersionProvider provider = new(wrappedOptions);

        string result = provider.GetVersion();

        Assert.Equal("1.2.3", result);
    }
}
