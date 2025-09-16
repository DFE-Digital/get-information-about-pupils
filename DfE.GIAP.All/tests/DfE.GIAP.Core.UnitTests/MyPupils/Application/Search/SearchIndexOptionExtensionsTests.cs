using DfE.GIAP.Core.MyPupils.Application.Services.Search.Options;
using DfE.GIAP.Core.MyPupils.Application.Services.Search.Options.Extensions;

namespace DfE.GIAP.Core.UnitTests.MyPupils.Application.Search;
public sealed class SearchIndexOptionExtensionsTests
{
    [Fact]
    public void GetIndexOptionsByName_ThrowsException_WhenIndexNameIsMissingOrNull()
    {
        // Arrange
        SearchIndexOptions options = new()
        {
            Indexes = new Dictionary<string, IndexOptions>
            {
                { "valid-index", new() },
                { "null-index", null! }
            }
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => options.GetIndexOptionsByName("missing-index"));
        Assert.Throws<ArgumentException>(() => options.GetIndexOptionsByName("null-index"));
    }
}
