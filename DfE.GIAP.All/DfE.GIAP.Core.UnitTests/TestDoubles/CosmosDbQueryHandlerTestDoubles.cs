using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Query;
using DfE.GIAP.Core.NewsArticles.Infrastructure.Repositories;

namespace DfE.GIAP.Core.UnitTests.TestDoubles;

internal static class CosmosDbQueryHandlerTestDoubles
{
    internal static Mock<ICosmosDbQueryHandler> Default() => new();

    internal static Mock<ICosmosDbQueryHandler> MockForGetNewsArticleById(Func<NewsArticleDto> handler)
    {
        Mock<ICosmosDbQueryHandler> mockHandler = Default();

        mockHandler
            .Setup(t => t.ReadItemByIdAsync<NewsArticleDto>(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(handler).Verifiable();

        return mockHandler;
    }

    internal static Mock<ICosmosDbQueryHandler> MockForGetNewsArticles(Func<IEnumerable<NewsArticleDto>> handler)
    {
        Mock<ICosmosDbQueryHandler> mockHandler = Default();

        mockHandler
            .Setup(t => t.ReadItemsAsync<NewsArticleDto>(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(handler)
            .Verifiable();

        return mockHandler;
    }
}
