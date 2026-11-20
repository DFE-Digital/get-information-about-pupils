using DfE.GIAP.Core.Common.Infrastructure;
using DfE.GIAP.Core.Common.Infrastructure.BlobStorage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Core.UnitTests.Common.Infrastructure.BlobStorage;

public sealed class BlobStorageOptionsValidationTests
{
    private static readonly Dictionary<string, string?> _validSettings = new()
    {
        ["BlobStorageOptions:AccountName"] = "account",
        ["BlobStorageOptions:AccountKey"] = "key",
        ["BlobStorageOptions:ContainerName"] = "container",
        ["BlobStorageOptions:EndpointSuffix"] = "core.windows.net"
    };

    [Fact]
    public void Options_AllSet_AreValid()
    {
        BlobStorageOptions options = GetOptions(_validSettings);

        Assert.Equal("account", options.AccountName);
    }

    [Theory]
    [InlineData(nameof(BlobStorageOptions.AccountName), null)]
    [InlineData(nameof(BlobStorageOptions.AccountKey), "")]
    [InlineData(nameof(BlobStorageOptions.ContainerName), " ")]
    [InlineData(nameof(BlobStorageOptions.EndpointSuffix), null)]
    public void Options_MissingOrBlank_AreInvalid(string property, string? value)
    {
        Dictionary<string, string?> settings = new(_validSettings);
        if (value is null)
        {
            settings.Remove($"BlobStorageOptions:{property}");
        }
        else
        {
            settings[$"BlobStorageOptions:{property}"] = value;
        }

        OptionsValidationException exception = Assert.Throws<OptionsValidationException>(() => GetOptions(settings));

        Assert.Contains(property, Assert.Single(exception.Failures));
    }

    private static BlobStorageOptions GetOptions(Dictionary<string, string?> settings)
    {
        ServiceCollection services = new();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(settings).Build());
        services.AddBlobStorageProvider();

        return services.BuildServiceProvider().GetRequiredService<IOptions<BlobStorageOptions>>().Value;
    }
}
