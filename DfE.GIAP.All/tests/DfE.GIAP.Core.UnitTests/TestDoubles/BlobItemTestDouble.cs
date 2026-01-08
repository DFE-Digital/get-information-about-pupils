using Azure;
using Azure.Storage.Blobs.Models;

namespace DfE.GIAP.Core.UnitTests.TestDoubles;
public static class BlobItemTestDouble
{
    public static BlobItem CreateItemWith(string name, string? contentType = null)
    {
        return BlobsModelFactory.BlobItem(
                    name: name,
                    properties: BlobsModelFactory.BlobItemProperties(
                        accessTierInferred: false, contentType: contentType));
    }

    public static AsyncPageable<BlobItem> AsyncPageableItemsFrom(IReadOnlyList<BlobItem> items)
    {
        return AsyncPageable<BlobItem>
            .FromPages(new[] { Page<BlobItem>.FromValues(items, null, Mock.Of<Response>()) });
    }

}
