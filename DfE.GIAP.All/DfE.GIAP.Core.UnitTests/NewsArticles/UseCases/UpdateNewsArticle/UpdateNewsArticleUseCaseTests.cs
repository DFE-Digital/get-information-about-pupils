using DfE.GIAP.Core.NewsArticles.Application.UseCases.CreateNewsArticle;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.UpdateNewsArticle;

namespace DfE.GIAP.Core.UnitTests.NewsArticles.UseCases.UpdateNewsArticle;
public sealed class UpdateNewsArticleUseCaseTests
{
    [Fact]
    public void Constructor_ThrowsNullException_When_CreatedWithNullRepository()
    {
        // Arrange
        Action construct = () => new UpdateNewsArticleUseCase(newsArticleWriteRepository: null!);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public async Task HandleRequestAsync_ThrowsNullException_When_RequestIsNull()
    {
        Mock<INewsArticleWriteRepository> mockRepository = NewsArticleWriteOnlyRepositoryTestDoubles.Default();
        UpdateNewsArticleUseCase sut = new(mockRepository.Object);

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

        UpdateNewsArticleUseCase sut = new(mockRepository.Object);
        UpdateNewsArticleRequest request = new(NewsArticle: null!);

        Func<Task> act = () => sut.HandleRequestAsync(request);

        // Act Assert
        await Assert.ThrowsAsync<ArgumentNullException>(act);
        mockRepository.Verify(
            (useCase) => useCase.UpdateNewsArticleAsync(It.IsAny<NewsArticle>()), Times.Never());
    }

    [Fact]
    public async Task HandleRequestAsync_CallsRepository_When_ArticleIsValid()
    {
        // Arrange
        Mock<INewsArticleWriteRepository> mockRepository = NewsArticleWriteOnlyRepositoryTestDoubles.Default();
        UpdateNewsArticleUseCase sut = new(mockRepository.Object);

        UpdateNewsArticleRequest request = new(NewsArticleTestDoubles.Create());

        await sut.HandleRequestAsync(request: request);

        // Act Assert
        mockRepository.Verify(
            (useCase) => useCase.UpdateNewsArticleAsync(It.IsAny<NewsArticle>()), Times.Once());
    }
}
