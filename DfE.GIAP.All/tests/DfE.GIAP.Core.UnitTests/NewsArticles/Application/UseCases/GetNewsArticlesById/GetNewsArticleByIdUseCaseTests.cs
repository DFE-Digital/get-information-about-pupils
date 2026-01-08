using DfE.GIAP.Core.UnitTests.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.NewsArticles.Application.UseCases.GetNewsArticlesById;

public sealed class GetNewsArticleByIdUseCaseTests
{
    private readonly string _validId = "any_valid_id";

    [Fact]
    public void Constructor_ThrowsNullException_When_CreatedWithNullRepository()
    {
        // Arrange
        Func<GetNewsArticleByIdUseCase> construct = () => new GetNewsArticleByIdUseCase(newsArticleReadRepository: null!);

        // Act
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public async Task HandleRequest_ThrowsNullException_When_RequestIsNull()
    {
        Mock<INewsArticleReadOnlyRepository> mockRepository = NewsArticleReadOnlyRepositoryTestDoubles.Default();
        GetNewsArticleByIdUseCase sut = new(mockRepository.Object);
        Func<Task> act = () => sut.HandleRequestAsync(request: null!);

        // Act Assert 
        await Assert.ThrowsAsync<ArgumentNullException>(act);
    }


    [Fact]
    public async Task HandleRequest_BubblesException_When_RepositoryThrows()
    {
        // Arrange
        const string expectedExceptionMessage = "Error occurs";
        INewsArticleReadOnlyRepository mockRepository =
            NewsArticleReadOnlyRepositoryTestDoubles.MockForGetNewsArticleById(() => throw new Exception(expectedExceptionMessage));
        GetNewsArticleByIdUseCase sut = new(mockRepository);
        GetNewsArticleByIdRequest request = new(NewsArticleIdentifier.From(_validId));
        Func<Task> act = () => sut.HandleRequestAsync(request);

        // Act Assert
        Exception exception = await Assert.ThrowsAsync<Exception>(act);
        Assert.Equal(expectedExceptionMessage, exception.Message);
    }

    [Fact]
    public async Task HandleRequest_Returns_NullNewsArticle_When_NotFound()
    {
        // Arrange
        NewsArticle? repositoryResponse = null;
        INewsArticleReadOnlyRepository mockRepository = NewsArticleReadOnlyRepositoryTestDoubles.MockFor(repositoryResponse);
        GetNewsArticleByIdUseCase sut = new(mockRepository);
        GetNewsArticleByIdRequest request = new(NewsArticleIdentifier.From(_validId));

        // Act
        GetNewsArticleByIdResponse response = await sut.HandleRequestAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Null(response.NewsArticle);
    }

    [Fact]
    public async Task HandleRequest_Returns_NewsArticle_When_Found()
    {
        // Arrange
        NewsArticle? repositoryResponse = NewsArticleTestDoubles.Create();
        INewsArticleReadOnlyRepository mockRepository = NewsArticleReadOnlyRepositoryTestDoubles.MockFor(repositoryResponse);
        GetNewsArticleByIdUseCase sut = new(mockRepository);
        GetNewsArticleByIdRequest request = new(NewsArticleIdentifier.From(_validId));

        // Act
        GetNewsArticleByIdResponse response = await sut.HandleRequestAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.NewsArticle);
        Assert.Equal(repositoryResponse, response.NewsArticle);
    }
}
