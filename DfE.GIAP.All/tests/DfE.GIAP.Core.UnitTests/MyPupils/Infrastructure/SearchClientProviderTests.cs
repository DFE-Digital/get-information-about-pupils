using DfE.GIAP.Core.MyPupils.Infrastructure.Search;
using DfE.GIAP.Core.Search.Infrastructure.Shared.Options;
using DfE.GIAP.SharedTests.Runtime.TestDoubles;
using Microsoft.Extensions.Options;


namespace DfE.GIAP.Core.UnitTests.MyPupils.Infrastructure;
public sealed class SearchClientProviderTests
{
    [Fact]
    public void Constructor_ThrowsException_WhenNoSearchClientsRegistered()
    {
        // Arrange
        IOptions<AzureSearchOptions> options = OptionsTestDoubles.Default<AzureSearchOptions>();

        // Act Assert
        Func<SearchClientProvider> act = () => new SearchClientProvider([], options);
        Assert.Throws<ArgumentException>(act);
    }
}

