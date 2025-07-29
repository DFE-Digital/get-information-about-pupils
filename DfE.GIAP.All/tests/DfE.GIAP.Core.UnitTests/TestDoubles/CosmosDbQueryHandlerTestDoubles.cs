using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Query;

namespace DfE.GIAP.Core.UnitTests.TestDoubles;

internal static class CosmosDbQueryHandlerTestDoubles
{
    internal static Mock<ICosmosDbQueryHandler> Default() => new();

    internal static Mock<ICosmosDbQueryHandler> MockForReadById<T>(Func<T> handler) where T : class
    {
        Mock<ICosmosDbQueryHandler> mockHandler = Default();

        mockHandler
            .Setup(t => t.ReadItemByIdAsync<T>(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(handler).Verifiable();

        return mockHandler;
    }

    internal static Mock<ICosmosDbQueryHandler> MockForReadMany<T>(Func<IEnumerable<T>> handler) where T : class
    {
        Mock<ICosmosDbQueryHandler> mockHandler = Default();

        mockHandler
            .Setup(t => t.ReadItemsAsync<T>(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(handler)
            .Verifiable();

        return mockHandler;
    }
}
