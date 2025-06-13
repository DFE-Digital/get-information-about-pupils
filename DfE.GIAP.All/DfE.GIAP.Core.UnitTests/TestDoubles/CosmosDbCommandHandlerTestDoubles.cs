// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Command;
using DfE.GIAP.Core.NewsArticles.Infrastructure.Repositories;

namespace DfE.GIAP.Core.UnitTests.TestDoubles;
internal static class CosmosDbCommandHandlerTestDoubles
{
    internal static Mock<ICosmosDbCommandHandler> Default() => new();

    internal static Mock<ICosmosDbCommandHandler> MockForCreateItemAsync(
        Func<NewsArticleDTO> handler)
    {
        Mock<ICosmosDbCommandHandler> mockHandler = Default();

        mockHandler
            .Setup(h => h.CreateItemAsync(
                It.IsAny<NewsArticleDTO>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(handler)
            .Verifiable();

        return mockHandler;
    }
}
