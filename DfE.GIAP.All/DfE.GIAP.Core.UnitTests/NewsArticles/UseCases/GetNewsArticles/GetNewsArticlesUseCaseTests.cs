using DfE.GIAP.Core.NewsArticles.Application.Enums;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticles;

namespace DfE.GIAP.Core.UnitTests.NewsArticles.UseCases.GetNewsArticles;

public sealed class GetNewsArticlesUseCaseTests
{
    [Fact]
    public void Constructor_ThrowsNullException_When_CreatedWithNullRepository()
    {
        // Arrange
        Action construct = () => new GetNewsArticlesUseCase(newsArticleReadRepository: null!);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public async Task HandleRequest_ThrowsNullException_When_RequestIsNull()
    {
        // Arrange
        Mock<INewsArticleReadRepository> mockRepository = NewsArticleReadOnlyRepositoryTestDoubles.Default();
        GetNewsArticlesUseCase sut = new(mockRepository.Object);
        Func<Task> act = () => sut.HandleRequestAsync(request: null);

        // Act Assert
        await Assert.ThrowsAsync<ArgumentNullException>(act);
    }

    [Fact]
    public async Task HandleRequest_BubblesException_When_RepositoryThrows()
    {
        // Arrange
        const string expectedExceptionMessage = "Error occurs";
        Mock<INewsArticleReadRepository> mockRepository =
            NewsArticleReadOnlyRepositoryTestDoubles.MockForGetNewsArticles(() => throw new Exception(expectedExceptionMessage));
        GetNewsArticlesUseCase sut = new(mockRepository.Object);
        GetNewsArticlesRequest request = new(It.IsAny<NewsArticleSearchFilter>());
        Func<Task> act = () => sut.HandleRequestAsync(request);

        // Act Assert
        Exception exception = await Assert.ThrowsAsync<Exception>(act);
        Assert.Equal(expectedExceptionMessage, exception.Message);
    }

    [Fact]
    public async Task HandleRequest_ReturnsEmpty_When_RepositoryReturnsEmpty()
    {
        // Arrange
        Mock<INewsArticleReadRepository> repo = NewsArticleReadOnlyRepositoryTestDoubles.MockForGetNewsArticles(Enumerable.Empty<NewsArticle>);
        GetNewsArticlesUseCase sut = new(repo.Object);
        GetNewsArticlesRequest request = new(It.IsAny<NewsArticleSearchFilter>());

        // Act
        GetNewsArticlesResponse response = await sut.HandleRequestAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Empty(response.NewsArticles);
    }

    [Fact]
    public async Task HandleRequest_ReturnsArticles_OrderedBy_Pinned_Desc_Then_ModifiedDate_Desc()
    {
        // Arrange
        // TODO put this behind an abstraction (static factory method? that can pass in a count of pinned articles)
        NewsArticle pinnedOldest = NewsArticleTestDoubles.Create();
        pinnedOldest.ModifiedDate = new DateTime(2021, 1, 3);
        pinnedOldest.Pinned = true;

        NewsArticle pinnedMiddle = NewsArticleTestDoubles.Create();
        pinnedOldest.ModifiedDate = new DateTime(2023, 6, 6);
        pinnedOldest.Pinned = true;

        NewsArticle pinnedNewest = NewsArticleTestDoubles.Create();
        pinnedOldest.ModifiedDate = new DateTime(2025, 1, 3);
        pinnedOldest.Pinned = true;

        NewsArticle unpinnedOldest = NewsArticleTestDoubles.Create();
        pinnedOldest.ModifiedDate = new DateTime(2024, 1, 10);
        unpinnedOldest.Pinned = false;

        NewsArticle unpinnedMiddle = NewsArticleTestDoubles.Create();
        unpinnedMiddle.ModifiedDate = new DateTime(2024, 2, 10);
        unpinnedMiddle.Pinned = false;

        NewsArticle unpinnedNewest = NewsArticleTestDoubles.Create();
        unpinnedNewest.ModifiedDate = new DateTime(2024, 3, 10);
        unpinnedNewest.Pinned = false;

        List<NewsArticle> unorderedUnpinnedArticles = [unpinnedOldest, pinnedMiddle, unpinnedNewest, pinnedOldest, pinnedNewest, unpinnedMiddle];

        Mock<INewsArticleReadRepository> repo = NewsArticleReadOnlyRepositoryTestDoubles.MockForGetNewsArticles(() => unorderedUnpinnedArticles);
        GetNewsArticlesUseCase sut = new(repo.Object);
        GetNewsArticlesRequest request = new(It.IsAny<NewsArticleSearchFilter>());

        // Act
        GetNewsArticlesResponse response = await sut.HandleRequestAsync(request);

        // Assert
        List<NewsArticle> expectedOrderArticles =
        [
            pinnedNewest,
            pinnedMiddle,
            pinnedOldest,
            unpinnedNewest,
            unpinnedMiddle,
            unpinnedOldest
        ];

        Assert.Equivalent(expectedOrderArticles, response.NewsArticles);
    }

    [Fact]
    public async Task HandleRequest_ReturnsArticles_OrderedBy_ModifiedDate_Desc_When_IsArchived_True()
    {
        // Arrange
        NewsArticle articleOldest = NewsArticleTestDoubles.Create();
        articleOldest.ModifiedDate = new DateTime(2021, 1, 1);

        NewsArticle articleMiddle = NewsArticleTestDoubles.Create();
        articleMiddle.ModifiedDate = new DateTime(2023, 6, 6);

        NewsArticle articleNewest = NewsArticleTestDoubles.Create();
        articleNewest.ModifiedDate = new DateTime(2025, 1, 1);

        // Pinned status should not affect order when IsArchived is true
        articleOldest.Pinned = true;
        articleMiddle.Pinned = false;
        articleNewest.Pinned = true;

        List<NewsArticle> unorderedArticles = [articleMiddle, articleOldest, articleNewest];

        Mock<INewsArticleReadRepository> repo = NewsArticleReadOnlyRepositoryTestDoubles.MockForGetNewsArticles(() => unorderedArticles);
        GetNewsArticlesUseCase sut = new(repo.Object);
        GetNewsArticlesRequest request = new(NewsArticleSearchFilter.ArchivedWithPublishedAndNotPublished);

        // Act
        GetNewsArticlesResponse response = await sut.HandleRequestAsync(request);

        // Assert
        List<NewsArticle> expectedOrder = [articleNewest, articleMiddle, articleOldest];
        Assert.Equivalent(expectedOrder, response.NewsArticles);
    }

    [Theory]
    [InlineData(NewsArticleSearchFilter.ArchivedWithPublished)]
    [InlineData(NewsArticleSearchFilter.ArchivedWithNotPublished)]
    [InlineData(NewsArticleSearchFilter.ArchivedWithPublishedAndNotPublished)]
    [InlineData(NewsArticleSearchFilter.NotArchivedWithPublished)]
    [InlineData(NewsArticleSearchFilter.NotArchivedWithNotPublished)]
    [InlineData(NewsArticleSearchFilter.NotArchivedWithPublishedAndNotPublished)]
    public async Task HandleRequest_CallsQueryHandler_Once_With_IsArchived_IsPublished(NewsArticleSearchFilter newsArticleSearchStatus)
    {
        Mock<INewsArticleReadRepository> repo = NewsArticleReadOnlyRepositoryTestDoubles.MockForGetNewsArticles(() => []);
        GetNewsArticlesUseCase sut = new(repo.Object);
        GetNewsArticlesRequest request = new(newsArticleSearchStatus);

        // Act
        GetNewsArticlesResponse response = await sut.HandleRequestAsync(request);

        // Assert
        Assert.NotNull(response);
        repo.Verify(
            (useCase) => useCase.GetNewsArticlesAsync(newsArticleSearchStatus), Times.Once());
    }
}
