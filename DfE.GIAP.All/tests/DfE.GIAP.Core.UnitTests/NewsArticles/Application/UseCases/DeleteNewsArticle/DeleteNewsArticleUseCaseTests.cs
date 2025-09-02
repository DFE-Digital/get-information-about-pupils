using DfE.GIAP.Core.NewsArticles.Application.UseCases.DeleteNewsArticle;
using DfE.GIAP.Core.UnitTests.NewsArticles.Application.UseCases;

namespace DfE.GIAP.Core.UnitTests.NewsArticles.Application.UseCases.DeleteNewsArticle;
public sealed class DeleteNewsArticleUseCaseTests
{
    [Fact]
    public void Constructor_ThrowsNullException_When_CreatedWithNullRepository()
    {
        // Arrange
        Action construct = () => new DeleteNewsArticleUseCase(newsArticleWriteRepository: null!);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public async Task HandleRequestAsync_ThrowsNullException_When_RequestIsNull()
    {
        Mock<INewsArticleWriteOnlyRepository> mockRepository = NewsArticleWriteOnlyRepositoryTestDoubles.Default();
        DeleteNewsArticleUseCase sut = new(mockRepository.Object);

        Func<Task> act = () => sut.HandleRequestAsync(request: null!);

        // Act Assert
        await Assert.ThrowsAsync<ArgumentNullException>(act);
        mockRepository.Verify(
            (useCase) => useCase.DeleteNewsArticleAsync(It.IsAny<NewsArticleIdentifier>()), Times.Never());
    }

    [Fact]
    public async Task HandleRequestAsync_CallsRepository_When_ArticleIdIsValid()
    {
        Mock<INewsArticleWriteOnlyRepository> mockRepository = NewsArticleWriteOnlyRepositoryTestDoubles.Default();
        DeleteNewsArticleUseCase sut = new(mockRepository.Object);

        DeleteNewsArticleRequest request = new(NewsArticleIdentifier.New());

        await sut.HandleRequestAsync(request: request);

        // Act Assert
        mockRepository.Verify(
            (useCase) => useCase.DeleteNewsArticleAsync(It.IsAny<NewsArticleIdentifier>()), Times.Once());
    }
}
