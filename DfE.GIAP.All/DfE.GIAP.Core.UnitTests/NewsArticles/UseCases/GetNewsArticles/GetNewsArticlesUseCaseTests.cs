using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.NewsArticles.Application.Services.NewsArticles;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticles;
using DfE.GIAP.Core.UnitTests.NewsArticles.UseCases.GetNewsArticlesById.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.NewsArticles.UseCases.GetNewsArticles;

public sealed class GetNewsArticlesUseCaseTests
{
    [Fact]
    public void Constructor_ThrowsNullException_When_CreatedWithNullRepository()
    {
        // Arrange
        Action construct = () => new GetNewsArticlesUseCase(
            newsArticleReadRepository: null,
            new Mock<IFilterNewsArticleSpecificationService>().Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_ThrowsNullException_When_CreatedWithNullFilterExpressionFactory()
    {
        // Arrange
        Action construct = () => new GetNewsArticlesUseCase(
            newsArticleReadRepository: NewsArticleReadOnlyRepositoryTestDoubles.Default().Object,
            filterNewsArticleSpecificationFactory: null!);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public async Task HandleRequest_ThrowsNullException_When_RequestIsNull()
    {
        // Arrange
        Mock<INewsArticleReadRepository> mockRepository = NewsArticleReadOnlyRepositoryTestDoubles.Default();
        Mock<IFilterNewsArticleSpecificationService> mockFilterSpecificationFactory = new Mock<IFilterNewsArticleSpecificationService>();
        GetNewsArticlesUseCase sut = new(mockRepository.Object, mockFilterSpecificationFactory.Object);
        Func<Task> act = () => sut.HandleRequest(request: null);

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
        Mock<IFilterNewsArticleSpecificationService> mockFilterSpecificationFactory = new Mock<IFilterNewsArticleSpecificationService>();
        mockFilterSpecificationFactory.Setup(t => t.Create(It.IsAny<IEnumerable<NewsArticleStateFilter>>())).Returns(It.IsAny<IFilterSpecification<NewsArticle>>());
        GetNewsArticlesUseCase sut = new(mockRepository.Object, mockFilterSpecificationFactory.Object);
        GetNewsArticlesRequest request = new(FilterNewsArticlesRequest.All());
        Func<Task> act = () => sut.HandleRequest(request);

        // Act Assert
        Exception exception = await Assert.ThrowsAsync<Exception>(act);
        Assert.Equal(expectedExceptionMessage, exception.Message);
    }

    [Fact]
    public async Task HandleRequest_ReturnsEmpty_When_RepositoryReturnsEmpty()
    {
        // Arrange
        Mock<INewsArticleReadRepository> repo = NewsArticleReadOnlyRepositoryTestDoubles.MockForGetNewsArticles(Enumerable.Empty<NewsArticle>);
        Mock<IFilterNewsArticleSpecificationService> mockFilterSpecificationFactory = new Mock<IFilterNewsArticleSpecificationService>();
        mockFilterSpecificationFactory.Setup(t => t.Create(It.IsAny<IEnumerable<NewsArticleStateFilter>>())).Returns(It.IsAny<IFilterSpecification<NewsArticle>>());

        GetNewsArticlesUseCase sut = new(repo.Object, mockFilterSpecificationFactory.Object);
        GetNewsArticlesRequest request = new(FilterNewsArticlesRequest.All());

        // Act
        GetNewsArticlesResponse response = await sut.HandleRequest(request);

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

        Mock<IFilterNewsArticleSpecificationService> mockFilterSpecificationFactory = new Mock<IFilterNewsArticleSpecificationService>();
        mockFilterSpecificationFactory.Setup(t => t.Create(It.IsAny<IEnumerable<NewsArticleStateFilter>>())).Returns(It.IsAny<IFilterSpecification<NewsArticle>>());
        Mock<INewsArticleReadRepository> repo = NewsArticleReadOnlyRepositoryTestDoubles.MockForGetNewsArticles(() => unorderedUnpinnedArticles);
        GetNewsArticlesUseCase sut = new(repo.Object, mockFilterSpecificationFactory.Object);
        GetNewsArticlesRequest request = new(FilterNewsArticlesRequest.All());

        // Act
        GetNewsArticlesResponse response = await sut.HandleRequest(request);

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

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public async Task HandleRequest_CallsQueryHandler_Once_With_IsArchived_IsDraft(bool isArchived, bool isDraft)
    {
        Mock<INewsArticleReadRepository> repo = NewsArticleReadOnlyRepositoryTestDoubles.MockForGetNewsArticles(() => []);
        Mock<IFilterNewsArticleSpecificationService> mockFilterSpecificationFactory = new Mock<IFilterNewsArticleSpecificationService>();
        mockFilterSpecificationFactory.Setup(t => t.Create(It.IsAny<IEnumerable<NewsArticleStateFilter>>())).Returns(It.IsAny<IFilterSpecification<NewsArticle>>());

        GetNewsArticlesUseCase sut = new(repo.Object, mockFilterSpecificationFactory.Object);
        GetNewsArticlesRequest request = new(FilterNewsArticlesRequest.All());

        // Act
        GetNewsArticlesResponse response = await sut.HandleRequest(request);

        // Assert
        Assert.NotNull(response);
        repo.Verify(
            (useCase) => useCase.GetNewsArticlesAsync(It.IsAny<IFilterSpecification<NewsArticle>>()), Times.Once());
    }
}
