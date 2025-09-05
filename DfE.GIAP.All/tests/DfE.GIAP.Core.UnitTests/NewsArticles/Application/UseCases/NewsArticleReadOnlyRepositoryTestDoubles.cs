using DfE.GIAP.Core.NewsArticles.Application.Enums;

namespace DfE.GIAP.Core.UnitTests.NewsArticles.Application.UseCases;

internal static class NewsArticleReadOnlyRepositoryTestDoubles
{
    internal static Mock<INewsArticleReadOnlyRepository> Default() => CreateMock();

    internal static INewsArticleReadOnlyRepository MockFor(NewsArticle? repositoryResponse = null)
    {
        Mock<INewsArticleReadOnlyRepository> mock = CreateMock();

        mock.Setup(
                (repository) => repository.GetNewsArticleByIdAsync(It.IsAny<string>()))
             .ReturnsAsync(repositoryResponse).Verifiable();

        return mock.Object;
    }

    internal static INewsArticleReadOnlyRepository MockForGetNewsArticleById(Func<NewsArticle> repositoryResponse)
    {
        Mock<INewsArticleReadOnlyRepository> mock = CreateMock();

        mock.Setup((repository) => repository.GetNewsArticleByIdAsync(It.IsAny<string>()))
             .ReturnsAsync(repositoryResponse).Verifiable();

        return mock.Object;
    }

    internal static Mock<INewsArticleReadOnlyRepository> MockForGetNewsArticles(Func<IEnumerable<NewsArticle>> repositoryResponse)
    {
        Mock<INewsArticleReadOnlyRepository> mock = CreateMock();

        mock.Setup(
                (repository) => repository.GetNewsArticlesAsync(It.IsAny<NewsArticleSearchFilter>()))
            .ReturnsAsync(repositoryResponse)
            .Verifiable();

        return mock;
    }

    private static Mock<INewsArticleReadOnlyRepository> CreateMock() => new();
}
