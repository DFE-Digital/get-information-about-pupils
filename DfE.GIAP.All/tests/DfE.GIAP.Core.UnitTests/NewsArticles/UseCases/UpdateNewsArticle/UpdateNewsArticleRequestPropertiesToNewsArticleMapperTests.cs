using DfE.GIAP.Core.Common.Application.TextSanitiser.Handlers;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.UpdateNewsArticle;

namespace DfE.GIAP.Core.UnitTests.NewsArticles.UseCases.UpdateNewsArticle;
public sealed class UpdateNewsArticleRequestPropertiesToNewsArticleMapperTests
{
    [Fact]
    public void Mapper_Throws_When_Input_Is_Null()
    {
        // Arrange
        UpdateNewsArticlesRequestPropertiesMapperToNewsArticle mapper = new();
        Func<NewsArticle> act = () => mapper.Map(null!);

        // Act Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void Mapper_Maps_DefaultProperties_When_Input_Properties_Is_Null()
    {
        // Arrange
        UpdateNewsArticlesRequestPropertiesMapperToNewsArticle mapper = new();
        UpdateNewsArticlesRequestProperties properties = new(id: "VALID_ID");

        // Act
        NewsArticle result = mapper.Map(properties);

        // Assert
        Assert.Equal(properties.Id, result.Id);
        Assert.Equal(string.Empty, result.Title);
        Assert.Equal(string.Empty, result.Body);
        Assert.Equal(properties.CreatedDate, result.CreatedDate);
        Assert.Equal(properties.ModifiedDate, result.ModifiedDate);
        Assert.False(result.Pinned);
        Assert.False(result.Published);
    }

    [Fact]
    public void Mapper_Maps_Properties_When_Input_Properties_Have_Values()
    {
        // Arrange
        UpdateNewsArticlesRequestPropertiesMapperToNewsArticle mapper = new();
        UpdateNewsArticlesRequestProperties properties = new(id: "VALID_ID")
        {
            Published = true,
            Pinned = true,
            Title = SanitisedTextResult.From("Test tile"),
            Body = SanitisedTextResult.From("Test body"),
        };

        // Act
        NewsArticle result = mapper.Map(properties);

        // Assert
        Assert.Equal(properties.Id, result.Id);
        Assert.Equal(properties.Title.Value, result.Title);
        Assert.Equal(properties.Body.Value, result.Body);
        Assert.Equal(properties.CreatedDate, result.CreatedDate);
        Assert.Equal(properties.ModifiedDate, result.ModifiedDate);
        Assert.Equal(properties.Pinned, result.Pinned);
        Assert.Equal(properties.Published, result.Published);
    }
}
