﻿using DfE.GIAP.Core.NewsArticles.Infrastructure.Repositories.DataTransferObjects;
using DfE.GIAP.Core.NewsArticles.Infrastructure.Repositories.Mappers;
using DfE.GIAP.Core.SharedTests.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.NewsArticles.Infrastructure.Repositories;

public sealed class NewsArticleDtoToEntityMapperTests
{
    [Fact]
    public void Map_ThrowsArgumentException_When_InputIsNull()
    {
        // Arrange
        NewsArticleDtoToEntityMapper mapper = new();

        // Act Assert
        Action act = () => mapper.Map(input: null!);
        Assert.Throws<ArgumentNullException>(act);
    }

    // TODO currently a direct map between DTO and Entity, all properties are type equivalent (no nullability)

    // TODO in future consider a way to validate fluently and public properties
    [Fact]
    public void Map_MapsProperties_When_DTO_HasProperties()
    {
        // Arrange
        NewsArticleDtoToEntityMapper mapper = new();
        NewsArticleDto inputDto = NewsArticleDtoTestDoubles.Generate(count: 1).Single();

        // Act Assert
        NewsArticle mappedResponse = mapper.Map(inputDto);
        Assert.NotNull(mappedResponse);
        Assert.Equal(mappedResponse.Id, NewsArticleIdentifier.From(inputDto.id));
        Assert.Equal(mappedResponse.Title, inputDto.Title);
        Assert.Equal(mappedResponse.Body, inputDto.Body);
        Assert.Equal(mappedResponse.CreatedDate, inputDto.CreatedDate);
        Assert.Equal(mappedResponse.ModifiedDate, inputDto.ModifiedDate);
        Assert.Equal(mappedResponse.Published, inputDto.Published);
        Assert.Equal(mappedResponse.Pinned, inputDto.Pinned);
    }
}
