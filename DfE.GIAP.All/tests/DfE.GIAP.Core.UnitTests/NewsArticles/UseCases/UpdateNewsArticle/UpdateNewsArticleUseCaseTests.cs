using System.Diagnostics;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.UpdateNewsArticle;
using DfE.GIAP.SharedTests.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.NewsArticles.UseCases.UpdateNewsArticle;
public sealed class UpdateNewsArticleUseCaseTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\n")]
    [InlineData("\r\n ")]
    public void UpdateNewsArticleRequestProperties_Throws_When_Identifier_Is_NullOrWhitespace(string? id)
    {
        Func<UpdateNewsArticlesRequestProperties> construct = () => new UpdateNewsArticlesRequestProperties(id!);
        Assert.ThrowsAny<ArgumentException>(construct);
    }

    [Fact]
    public void UpdateNewsArticleRequestProperties_Default_Properties_When_Constructed()
    {
        // Arrange
        DateTime utcDateBeforeCreation = DateTime.UtcNow;
        Stopwatch watch = Stopwatch.StartNew();

        // Act
        UpdateNewsArticlesRequestProperties properties = new(id: "VALID_ID");
        watch.Stop();

        // Assert
        Assert.Equal("VALID_ID", properties.Id.Value);
        Assert.InRange(properties.CreatedDate, utcDateBeforeCreation, utcDateBeforeCreation.Add(watch.Elapsed));
        Assert.InRange(properties.ModifiedDate, utcDateBeforeCreation, utcDateBeforeCreation.Add(watch.Elapsed));

        Assert.Null(properties.Title);
        Assert.Null(properties.Body);
        Assert.Null(properties.Pinned);
        Assert.Null(properties.Published);
    }

    [Fact]
    public void Constructor_ThrowsNullException_When_CreatedWithNullRepository()
    {
        // Arrange
        Mock<IMapper<UpdateNewsArticlesRequestProperties, NewsArticle>> mockMapper = MapperTestDoubles.Default<UpdateNewsArticlesRequestProperties, NewsArticle>();
        Func<UpdateNewsArticleUseCase> construct = () => new UpdateNewsArticleUseCase(newsArticleWriteRepository: null!, mockMapper.Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_ThrowsNullException_When_CreatedWithNullMapper()
    {
        // Arrange
        Mock<INewsArticleWriteRepository> mockRepository = NewsArticleWriteOnlyRepositoryTestDoubles.Default();
        Func<UpdateNewsArticleUseCase> construct = () => new UpdateNewsArticleUseCase(mockRepository.Object, null!);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public async Task HandleRequestAsync_ThrowsNullException_When_RequestIsNull()
    {
        Mock<IMapper<UpdateNewsArticlesRequestProperties, NewsArticle>> mockMapper = MapperTestDoubles.Default<UpdateNewsArticlesRequestProperties, NewsArticle>();
        Mock<INewsArticleWriteRepository> mockRepository = NewsArticleWriteOnlyRepositoryTestDoubles.Default();
        UpdateNewsArticleUseCase sut = new(mockRepository.Object, mockMapper.Object);

        Func<Task> act = () => sut.HandleRequestAsync(request: null!);

        // Act Assert
        await Assert.ThrowsAsync<ArgumentNullException>(act);
        mockRepository.Verify(
            (useCase) => useCase.UpdateNewsArticleAsync(It.IsAny<NewsArticle>()), Times.Never());
    }

    [Fact]
    public async Task HandleRequestAsync_ThrowsNullException_When_NewsArticleIsNull()
    {
        Mock<INewsArticleWriteRepository> mockRepository = NewsArticleWriteOnlyRepositoryTestDoubles.Default();
        Mock<IMapper<UpdateNewsArticlesRequestProperties, NewsArticle>> mockMapper = MapperTestDoubles.Default<UpdateNewsArticlesRequestProperties, NewsArticle>();

        UpdateNewsArticleUseCase sut = new(mockRepository.Object, mockMapper.Object);
        UpdateNewsArticleRequest request = new(UpdateArticleProperties: null!);

        Func<Task> act = () => sut.HandleRequestAsync(request);

        // Act Assert
        await Assert.ThrowsAsync<ArgumentNullException>(act);
        mockRepository.Verify(
            (useCase) => useCase.UpdateNewsArticleAsync(It.IsAny<NewsArticle>()), Times.Never());
    }

    [Fact]
    public async Task HandleRequestAsync_Calls_Repository_AndMapper_When_UpdateIsValid()
    {
        // Arrange
        NewsArticle stub = NewsArticleTestDoubles.Create();
        Mock<IMapper<UpdateNewsArticlesRequestProperties, NewsArticle>> mockMapper = MapperTestDoubles.MockFor<UpdateNewsArticlesRequestProperties, NewsArticle>(() => stub);
        Mock<INewsArticleWriteRepository> mockRepository = NewsArticleWriteOnlyRepositoryTestDoubles.Default();
        UpdateNewsArticleUseCase sut = new(mockRepository.Object, mockMapper.Object);

        UpdateNewsArticlesRequestProperties requestProperties = new(id: "Any valid id");
        UpdateNewsArticleRequest request = new(requestProperties);

        await sut.HandleRequestAsync(request);

        // Act Assert
        mockRepository.Verify(
            (useCase) => useCase.UpdateNewsArticleAsync(It.IsAny<NewsArticle>()), Times.Once());

        mockMapper.Verify(
            mapper => mapper.Map(It.IsAny<UpdateNewsArticlesRequestProperties>()), Times.Once);
    }
}
