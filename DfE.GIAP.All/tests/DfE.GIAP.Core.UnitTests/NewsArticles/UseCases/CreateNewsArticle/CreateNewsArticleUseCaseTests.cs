using DfE.GIAP.Core.NewsArticles.Application.UseCases.CreateNewsArticle;

namespace DfE.GIAP.Core.UnitTests.NewsArticles.UseCases.CreateNewsArticle;
public sealed class CreateNewsArticleUseCaseTests
{
    [Fact]
    public void Constructor_ThrowsNullException_When_CreatedWithNullRepository()
    {
        // Arrange
        Func<CreateNewsArticleUseCase> construct = () => new CreateNewsArticleUseCase(newsArticleWriteRepository: null!);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public async Task HandleRequestAsync_ThrowsNullException_When_RequestIsNull()
    {
        Mock<INewsArticleWriteOnlyRepository> mockRepository = NewsArticleWriteOnlyRepositoryTestDoubles.Default();
        CreateNewsArticleUseCase sut = new(mockRepository.Object);

        Func<Task> act = () => sut.HandleRequestAsync(request: null!);

        // Act Assert
        await Assert.ThrowsAsync<ArgumentNullException>(act);
        mockRepository.Verify(
            (useCase) => useCase.CreateNewsArticleAsync(It.IsAny<NewsArticle>()), Times.Never());
    }

    [Fact]
    public async Task HandleRequestAsync_ThrowsArgumentException_When_TitleIsNull()
    {
        Mock<INewsArticleWriteOnlyRepository> mockRepository = NewsArticleWriteOnlyRepositoryTestDoubles.Default();
        CreateNewsArticleUseCase sut = new(mockRepository.Object);

        CreateNewsArticleRequest request = new(null!, "body", true, true);
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
        Mock<INewsArticleWriteOnlyRepository> mockRepository = NewsArticleWriteOnlyRepositoryTestDoubles.Default();
        CreateNewsArticleUseCase sut = new(mockRepository.Object);

        CreateNewsArticleRequest request = new(title, "body", true, true);
        Func<Task> act = () => sut.HandleRequestAsync(request: request);

        // Act Assert
        await Assert.ThrowsAsync<ArgumentException>(act);
        mockRepository.Verify(
            (useCase) => useCase.CreateNewsArticleAsync(It.IsAny<NewsArticle>()), Times.Never());
    }

    [Fact]
    public async Task HandleRequestAsync_ThrowsArgumentException_When_BodyIsNull()
    {
        Mock<INewsArticleWriteOnlyRepository> mockRepository = NewsArticleWriteOnlyRepositoryTestDoubles.Default();
        CreateNewsArticleUseCase sut = new(mockRepository.Object);

        CreateNewsArticleRequest request = new("title", null!, true, true);

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
        Mock<INewsArticleWriteOnlyRepository> mockRepository = NewsArticleWriteOnlyRepositoryTestDoubles.Default();
        CreateNewsArticleUseCase sut = new(mockRepository.Object);

        CreateNewsArticleRequest request = new("title", body, true, true);

        Func<Task> act = () => sut.HandleRequestAsync(request: request);

        // Act Assert
        await Assert.ThrowsAsync<ArgumentException>(act);
        mockRepository.Verify(
            (useCase) => useCase.CreateNewsArticleAsync(It.IsAny<NewsArticle>()), Times.Never());
    }

    [Fact]
    public async Task HandleRequestAsync_CallsRepository_When_ArticleIsValid()
    {
        Mock<INewsArticleWriteOnlyRepository> mockRepository = NewsArticleWriteOnlyRepositoryTestDoubles.Default();
        CreateNewsArticleUseCase sut = new(mockRepository.Object);

        CreateNewsArticleRequest request = new("title", "body", true, true);

        await sut.HandleRequestAsync(request: request);

        // Act Assert
        mockRepository.Verify(
            (useCase) => useCase.CreateNewsArticleAsync(It.IsAny<NewsArticle>()), Times.Once());
    }
}
