using DfE.GIAP.Core.Search.Application.Models.Search;
using FluentAssertions;

namespace DfE.GIAP.Core.UnitTests.Search.Application.Models.Search;

public sealed class SearchCriteriaTests
{
    [Fact]
    public void Constructor_ShouldInitializeEmptyCollections()
    {
        // act
        SearchCriteria criteria = new();

        // Assert
        criteria.SearchFields.Should().NotBeNull().And.BeEmpty();
        criteria.Facets.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Properties_CanBeAssignedAndRetrieved()
    {
        // arrange
        List<string> fields = ["Name", "Subject"];
        List<string> facets = ["Region", "Provider"];

        SearchCriteria criteria = new()
        {
            SearchFields = fields,
            Facets = facets
        };

        // Assert
        criteria.SearchFields.Should().BeEquivalentTo(fields);
        criteria.Facets.Should().BeEquivalentTo(facets);
    }

    [Fact]
    public void Properties_CanBeMutatedAfterInitialization()
    {
        // arrange
        SearchCriteria criteria = new();

        // act
        criteria.SearchFields.Add("Level");
        criteria.Facets.Add("Gender");

        // Assert
        criteria.SearchFields.Should().ContainSingle().Which.Should().Be("Level");
        criteria.Facets.Should().ContainSingle().Which.Should().Be("Gender");
    }
}

