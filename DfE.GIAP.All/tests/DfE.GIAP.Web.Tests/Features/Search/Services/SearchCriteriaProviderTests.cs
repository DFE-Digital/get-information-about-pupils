using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.SharedTests.Runtime.TestDoubles;
using DfE.GIAP.Web.Features.Search.Options.Search;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Web.Tests.Features.Search.Services;

public sealed class SearchCriteriaProviderTests
{

    [Fact]
    public void Constructor_Throws_When_Options_Is_Null()
    {
        // Arrange
        Func<SearchCriteriaProvider> construct = () => new SearchCriteriaProvider(null!);

        // Act & Assert
        Assert.ThrowsAny<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_Options_Value_Is_Null()
    {
        // Arrange
        IOptions<SearchCriteriaOptions> options = OptionsTestDoubles.MockNullOptions<SearchCriteriaOptions>(); 
        Func<SearchCriteriaProvider> construct = () => new SearchCriteriaProvider(options);

        // Act & Assert
        Assert.ThrowsAny<ArgumentNullException>(construct);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void GetCriteria_Throws_When_Key_Is_Null_Or_WhiteSpace(string? key)
    {
        // Arrange
        SearchCriteriaOptions searchCriteriaOptions = CreateOptionsWithCriteria([]);
        IOptions<SearchCriteriaOptions> options = OptionsTestDoubles.MockAs(searchCriteriaOptions);

        SearchCriteriaProvider provider = new(options);
        Func<SearchCriteria> act = () => provider.GetCriteria(key!);

        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(act);
    }

    [Fact]
    public void GetCriteria_Throws_When_Key_DoesNot_Exist_In_Options_Criteria()
    {
        // Arrange
        SearchCriteria expected = new SearchCriteria();

        Dictionary<string, SearchCriteria> criteria = new()
        {
            { "known-key", expected }
        };

        SearchCriteriaOptions searchCriteriaOptions = CreateOptionsWithCriteria(criteria);
        IOptions<SearchCriteriaOptions> options = OptionsTestDoubles.MockAs(searchCriteriaOptions);

        SearchCriteriaProvider provider = new(options);
        Func<SearchCriteria> act = () => provider.GetCriteria("missing-key");

        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(act);
    }

    [Fact]
    public void GetCriteria_Returns_Criteria_When_Key_Exists_In_Options_Criteria()
    {
        // Arrange
        SearchCriteria expected = new();

        Dictionary<string, SearchCriteria> criteria = new Dictionary<string, SearchCriteria>
        {
            { "known-key", expected }
        };

        SearchCriteriaOptions searchCriteriaOptions = CreateOptionsWithCriteria(criteria);
        IOptions<SearchCriteriaOptions> options = OptionsTestDoubles.MockAs(searchCriteriaOptions);

        SearchCriteriaProvider provider = new(options);

        // Act
        SearchCriteria result = provider.GetCriteria("known-key");

        // Assert
        Assert.Same(expected, result);
    }

    private static SearchCriteriaOptions CreateOptionsWithCriteria(Dictionary<string, SearchCriteria> criteria)
    {
        // NOTE: assumes Criteria is settable. If it's init-only, switch to object initializer accordingly.
        SearchCriteriaOptions options = new()
        {
            Criteria = criteria
        };

        return options;
    }
}
