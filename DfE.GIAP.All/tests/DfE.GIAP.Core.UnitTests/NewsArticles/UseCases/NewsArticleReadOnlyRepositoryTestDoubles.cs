using DfE.GIAP.Core.NewsArticles.Application.Enums;

namespace DfE.GIAP.Core.UnitTests.NewsArticles.UseCases;

internal static class NewsArticleReadOnlyRepositoryTestDoubles
{
    internal static Mock<INewsArticleReadRepository> Default() => CreateMock();

    internal static INewsArticleReadRepository MockFor(NewsArticle? repositoryResponse = null)
    {
        Mock<INewsArticleReadRepository> mock = CreateMock();

        mock.Setup(
                (repository) => repository.GetNewsArticleByIdAsync(It.IsAny<string>()))
             .ReturnsAsync(repositoryResponse).Verifiable();

        return mock.Object;
    }

    internal static INewsArticleReadRepository MockForGetNewsArticleById(Func<NewsArticle> repositoryResponse)
    {
        Mock<INewsArticleReadRepository> mock = CreateMock();

        mock.Setup((repository) => repository.GetNewsArticleByIdAsync(It.IsAny<string>()))
             .ReturnsAsync(repositoryResponse).Verifiable();

        return mock.Object;
    }

    internal static Mock<INewsArticleReadRepository> MockForGetNewsArticles(Func<IEnumerable<NewsArticle>> repositoryResponse)
    {
        Mock<INewsArticleReadRepository> mock = CreateMock();

        mock.Setup(
                (repository) => repository.GetNewsArticlesAsync(It.IsAny<NewsArticleSearchFilter>()))
            .ReturnsAsync(repositoryResponse)
            .Verifiable();

        return mock;
    }

    private static Mock<INewsArticleReadRepository> CreateMock() => new();
}
