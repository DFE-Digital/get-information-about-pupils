using DfE.GIAP.Core.NewsArticles.Application.UseCases.CreateNewsArticle;

namespace DfE.GIAP.Core.UnitTests.NewsArticles.UseCases.CreateNewsArticle;
public sealed class CreateNewsArticleUseCaseTests
{
    [Fact]
    public void Constructor_ThrowsNullException_When_CreatedWithNullRepository()
    {
        // Arrange
        Action construct = () => new CreateNewsArticleUseCase(newsArticleWriteRepository: null);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public async Task HandleRequestAsync_ThrowsNullException_When_RequestIsNull()
    {
        Mock<INewsArticleWriteRepository> mockRepository = NewsArticleWriteOnlyRepositoryTestDoubles.Default();
        CreateNewsArticleUseCase sut = new(mockRepository.Object);

        Func<Task> act = () => sut.HandleRequestAsync(request: null);

        // Act Assert
        await Assert.ThrowsAsync<ArgumentNullException>(act);
        mockRepository.Verify(
            (useCase) => useCase.CreateNewsArticleAsync(It.IsAny<NewsArticle>()), Times.Never());
    }

    [Theory]
    [InlineData(null)]
    public async Task HandleRequestAsync_ThrowsArgumentException_When_TitleIsNull(string? title)
    {
        Mock<INewsArticleWriteRepository> mockRepository = NewsArticleWriteOnlyRepositoryTestDoubles.Default();
        CreateNewsArticleUseCase sut = new(mockRepository.Object);

        CreateNewsArticleRequest request = new(title, "body", true, true, true);
        Func<Task> act = () => sut.HandleRequestAsync(request: request);

        // Act Assert
        await Assert.ThrowsAsync<ArgumentNullException>(act);
        mockRepository.Verify(
            (useCase) => useCase.CreateNewsArticleAsync(It.IsAny<NewsArticle>()), Times.Never());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\n")]
    public async Task HandleRequestAsync_ThrowsArgumentException_When_TitleIsEmpty(string title)
    {
        Mock<INewsArticleWriteRepository> mockRepository = NewsArticleWriteOnlyRepositoryTestDoubles.Default();
        CreateNewsArticleUseCase sut = new(mockRepository.Object);

        CreateNewsArticleRequest request = new(title, "body", true, true, true);
        Func<Task> act = () => sut.HandleRequestAsync(request: request);

        // Act Assert
        await Assert.ThrowsAsync<ArgumentException>(act);
        mockRepository.Verify(
            (useCase) => useCase.CreateNewsArticleAsync(It.IsAny<NewsArticle>()), Times.Never());
    }

    [Theory]
    [InlineData(null)]
    public async Task HandleRequestAsync_ThrowsArgumentException_When_BodyIsNull(string? body)
    {
        Mock<INewsArticleWriteRepository> mockRepository = NewsArticleWriteOnlyRepositoryTestDoubles.Default();
        CreateNewsArticleUseCase sut = new(mockRepository.Object);

        CreateNewsArticleRequest request = new("title", body, true, true, true);

        Func<Task> act = () => sut.HandleRequestAsync(request: request);

        // Act Assert
        await Assert.ThrowsAsync<ArgumentNullException>(act);
        mockRepository.Verify(
            (useCase) => useCase.CreateNewsArticleAsync(It.IsAny<NewsArticle>()), Times.Never());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\n")]
    public async Task HandleRequestAsync_ThrowsArgumentException_When_BodyIsEmpty(string body)
    {
        Mock<INewsArticleWriteRepository> mockRepository = NewsArticleWriteOnlyRepositoryTestDoubles.Default();
        CreateNewsArticleUseCase sut = new(mockRepository.Object);

        CreateNewsArticleRequest request = new("title", body, true, true, true);

        Func<Task> act = () => sut.HandleRequestAsync(request: request);

        // Act Assert
        await Assert.ThrowsAsync<ArgumentException>(act);
        mockRepository.Verify(
            (useCase) => useCase.CreateNewsArticleAsync(It.IsAny<NewsArticle>()), Times.Never());
    }

    [Fact]
    public async Task HandleRequestAsync_CallsRepository_When_ArticleIsValid()
    {
        Mock<INewsArticleWriteRepository> mockRepository = NewsArticleWriteOnlyRepositoryTestDoubles.Default();
        CreateNewsArticleUseCase sut = new(mockRepository.Object);

        CreateNewsArticleRequest request = new("title", "body", true, true, true);

        await sut.HandleRequestAsync(request: request);

        // Act Assert
        mockRepository.Verify(
            (useCase) => useCase.CreateNewsArticleAsync(It.IsAny<NewsArticle>()), Times.Once());
    }
}
