using DfE.GIAP.Core.MyPupils.Application.Search.Options;
using DfE.GIAP.Core.MyPupils.Infrastructure.Search;
using DfE.GIAP.SharedTests.TestDoubles;
using Microsoft.Extensions.Options;


namespace DfE.GIAP.Core.UnitTests.MyPupils.Application.Search;
public sealed class SearchClientProviderTests
{
    [Fact]
    public void Constructor_ThrowsException_WhenNoSearchClientsRegistered()
    {
        // Arrange
        IOptions<SearchIndexOptions> options = OptionsTestDoubles.Default<SearchIndexOptions>();

        // Act Assert
        Func<SearchClientProvider> act = () => new SearchClientProvider([], options);
        Assert.Throws<ArgumentException>(act);
    }
}

