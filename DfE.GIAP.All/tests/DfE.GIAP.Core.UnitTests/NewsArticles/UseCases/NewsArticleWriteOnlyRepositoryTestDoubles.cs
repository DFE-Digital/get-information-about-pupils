namespace DfE.GIAP.Core.UnitTests.NewsArticles.UseCases;
internal static class NewsArticleWriteOnlyRepositoryTestDoubles
{
    internal static Mock<INewsArticleWriteRepository> Default() => CreateMock();

    private static Mock<INewsArticleWriteRepository> CreateMock() => new();
}
