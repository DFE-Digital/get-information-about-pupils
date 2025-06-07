using DfE.GIAP.Core.NewsArticles.Infrastructure.Repositories;
using DfE.GIAP.Core.NewsArticles.Infrastructure.Repositories.Mappers;
using DfE.GIAP.Core.SharedTests.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.NewsArticles.Infrastructure.Repositories;

public sealed class NewsArticleDTOToEntityMapperTests
{

    [Fact]
    public void NewsArticleDTOToEntityMapper_Construct_With_Default_Constructor() => new NewsArticleDTOToEntityMapper();

    [Fact]
    public void NewsArticleDTOToEntityMapper_ThrowsArgumentException_When_InputIsNull()
    {
        // Arrange
        NewsArticleDTOToEntityMapper mapper = new();

        // Act Assert
        Action act = () => mapper.Map(input: null!);
        Assert.Throws<ArgumentNullException>(act);
    }

    // TODO currently a direct map between DTO and Entity, all properties are type equivalent (no nullability)

    // TODO in future consider a way to validate fluently and public properties
    [Fact]
    public void NewsArticleDTOToEntityMapper_MapsProperties_When_DTO_HasProperties()
    {
        // Arrange
        NewsArticleDTOToEntityMapper mapper = new();
        NewsArticleDTO inputDto = NewsArticleDTOTestDoubles.Generate(count: 1).Single();

        // Act Assert
        NewsArticle mappedResponse = mapper.Map(inputDto);
        Assert.NotNull(mappedResponse);
        Assert.Equal(mappedResponse.Id, inputDto.ID);
        Assert.Equal(mappedResponse.Title, inputDto.Title);
        Assert.Equal(mappedResponse.Body, inputDto.Body);
        Assert.Equal(mappedResponse.DraftBody, inputDto.DraftBody);
        Assert.Equal(mappedResponse.DraftTitle, inputDto.DraftTitle);
        Assert.Equal(mappedResponse.CreatedDate, inputDto.CreatedDate);
        Assert.Equal(mappedResponse.ModifiedDate, inputDto.ModifiedDate);
        Assert.Equal(mappedResponse.Published, inputDto.Published);
        Assert.Equal(mappedResponse.Archived, inputDto.Archived);
        Assert.Equal(mappedResponse.Pinned, inputDto.Pinned);
    }
}
