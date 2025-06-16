using DfE.GIAP.Core.NewsArticles.Infrastructure.Repositories;
using DfE.GIAP.Core.NewsArticles.Infrastructure.Repositories.Mappers;
using DfE.GIAP.Core.UnitTests.NewsArticles.UseCases;

namespace DfE.GIAP.Core.UnitTests.NewsArticles.Infrastructure.Repositories;

public sealed class NewsArticleEntityToDtoMapperTests
{
    [Fact]
    public void NewsArticleEntityToDTOMapper_Construct_With_Default_Constructor() => new NewsArticleEntityToDtoMapper();

    [Fact]
    public void Map_ThrowsArgumentException_When_InputIsNull()
    {
        // Arrange
        NewsArticleEntityToDtoMapper mapper = new();

        // Act Assert
        Action act = () => mapper.Map(input: null!);
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void Map_MapsProperties_When_Entity_HasProperties()
    {
        // Arrange
        NewsArticleEntityToDtoMapper mapper = new();
        NewsArticle inputEntity = NewsArticleTestDoubles.Create();

        // Act Assert
        NewsArticleDTO mappedResponse = mapper.Map(inputEntity);
        Assert.NotNull(mappedResponse);
        Assert.Equal(mappedResponse.Id, inputEntity.Id.Value);
        Assert.Equal(mappedResponse.Title, inputEntity.Title);
        Assert.Equal(mappedResponse.Body, inputEntity.Body);
        Assert.Equal(mappedResponse.DraftBody, inputEntity.DraftBody);
        Assert.Equal(mappedResponse.DraftTitle, inputEntity.DraftTitle);
        Assert.Equal(mappedResponse.CreatedDate, inputEntity.CreatedDate);
        Assert.Equal(mappedResponse.ModifiedDate, inputEntity.ModifiedDate);
        Assert.Equal(mappedResponse.Published, inputEntity.Published);
        Assert.Equal(mappedResponse.Archived, inputEntity.Archived);
        Assert.Equal(mappedResponse.Pinned, inputEntity.Pinned);
        Assert.Equal(7, mappedResponse.DocumentType); // Temporary, to be removed when DocumentType is removed from DTOs
    }
}
