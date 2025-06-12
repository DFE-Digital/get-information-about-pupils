// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Command;

namespace DfE.GIAP.Core.UnitTests.TestDoubles;
internal static class CosmosDbCommandHandlerTestDoubles
{
    internal static Mock<ICosmosDbCommandHandler> Default() => new();
}
