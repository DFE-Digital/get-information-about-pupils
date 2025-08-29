namespace DfE.GIAP.Core.UnitTests.NewsArticles.UseCases;
internal static class NewsArticleWriteOnlyRepositoryTestDoubles
{
    internal static Mock<INewsArticleWriteOnlyRepository> Default() => CreateMock();

    private static Mock<INewsArticleWriteOnlyRepository> CreateMock() => new();
}
