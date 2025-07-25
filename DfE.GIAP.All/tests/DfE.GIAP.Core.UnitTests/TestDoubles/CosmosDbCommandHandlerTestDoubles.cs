﻿using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Command;
using DfE.GIAP.Core.NewsArticles.Infrastructure.Repositories;

namespace DfE.GIAP.Core.UnitTests.TestDoubles;
internal static class CosmosDbCommandHandlerTestDoubles
{
    internal static Mock<ICosmosDbCommandHandler> Default() => new();

    internal static Mock<ICosmosDbCommandHandler> MockForCreateItemAsync(
        Func<NewsArticleDto> handler)
    {
        Mock<ICosmosDbCommandHandler> mockHandler = Default();

        mockHandler
            .Setup(h => h.CreateItemAsync(
                It.IsAny<NewsArticleDto>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(handler)
            .Verifiable();

        return mockHandler;
    }

    internal static Mock<ICosmosDbCommandHandler> MockForDeleteItemAsync(Exception? exceptionToThrow = null)
    {
        Mock<ICosmosDbCommandHandler> mockHandler = Default();

        Moq.Language.Flow.ISetup<ICosmosDbCommandHandler, Task> setup = mockHandler
            .Setup(h => h.DeleteItemAsync<NewsArticleDto>(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(), // <-- string, not PartitionKey
                It.IsAny<CancellationToken>()));

        if (exceptionToThrow is not null)
        {
            setup.ThrowsAsync(exceptionToThrow);
        }
        else
        {
            setup.Returns(Task.CompletedTask);
        }

        setup.Verifiable();

        return mockHandler;
    }

    internal static Mock<ICosmosDbCommandHandler> MockForReplaceItemAsync(Exception? exceptionToThrow = null)
    {
        Mock<ICosmosDbCommandHandler> mockHandler = Default();

        Moq.Language.Flow.ISetup<ICosmosDbCommandHandler, Task<NewsArticleDto>> setup = mockHandler
            .Setup(h => h.ReplaceItemAsync(
                It.IsAny<NewsArticleDto>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()));

        if (exceptionToThrow != null)
        {
            setup.ThrowsAsync(exceptionToThrow);
        }
        else
        {
            setup.Verifiable();
        }

        return mockHandler;
    }

}
