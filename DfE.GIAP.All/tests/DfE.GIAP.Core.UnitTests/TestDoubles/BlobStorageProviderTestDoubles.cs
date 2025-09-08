using DfE.GIAP.Core.Common.Infrastructure.BlobStorage;

namespace DfE.GIAP.Core.UnitTests.TestDoubles;
internal static class BlobStorageProviderTestDoubles
{
    internal static Mock<IBlobStorageProvider> Default() => new();

    internal static IBlobStorageProvider MockForDownloadBlobAsStream(Func<Stream> repositoryResponse)
    {
        Mock<IBlobStorageProvider> mock = Default();

        mock.Setup((repository) => repository.DownloadBlobAsStreamAsync(It.IsAny<string>(), It.IsAny<string>(), default))
             .ReturnsAsync(repositoryResponse).Verifiable();

        return mock.Object;
    }

    internal static IBlobStorageProvider MockForListBlobWithMetadata(Func<IEnumerable<BlobItemMetadata>> repositoryResponse)
    {
        Mock<IBlobStorageProvider> mock = Default();

        mock.Setup((repository) => repository.ListBlobsWithMetadataAsync(It.IsAny<string>(), It.IsAny<string>(), default))
             .ReturnsAsync(repositoryResponse).Verifiable();

        return mock.Object;
    }
}
