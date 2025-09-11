namespace DfE.GIAP.Core.UnitTests.TestDoubles;
internal static class NewsArticleWriteOnlyRepositoryTestDoubles
{
    internal static Mock<INewsArticleWriteOnlyRepository> Default() => CreateMock();

    private static Mock<INewsArticleWriteOnlyRepository> CreateMock() => new();
}
