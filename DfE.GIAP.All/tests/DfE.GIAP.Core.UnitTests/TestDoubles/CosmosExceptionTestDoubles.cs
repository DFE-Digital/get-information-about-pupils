using System.Net;
using Microsoft.Azure.Cosmos;

namespace DfE.GIAP.Core.UnitTests.TestDoubles;

internal static class CosmosExceptionTestDoubles
{
    internal static CosmosException Default()
        => new(
            It.IsAny<string>(),
            It.IsAny<HttpStatusCode>(),
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<double>());

    // <T> allows us to pass this Func<T> to anything that should yield T.
    internal static Func<T> ThrowsCosmosExceptionDelegate<T>() => () => throw Default();
}
