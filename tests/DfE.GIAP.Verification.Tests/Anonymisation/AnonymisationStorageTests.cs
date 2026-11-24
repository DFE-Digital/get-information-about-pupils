using Azure.Storage.Blobs;
using DfE.GIAP.Verification.Anonymisation;

namespace DfE.GIAP.Verification.Tests.Anonymisation;

public class AnonymisationStorageTests
{
    private static readonly BlobContainerClient _productionSource = new(new Uri("https://giaptrxprod.blob.core.windows.net/giapdataprocessing?sig=x"));
    private static readonly BlobContainerClient _testDestination = new(new Uri("https://s189t01giaptsingestsa.blob.core.windows.net/verification?sig=x"));
    private static readonly BlobContainerClient _azuriteSource = new("UseDevelopmentStorage=true", "giapdataprocessing");
    private static readonly BlobContainerClient _azuriteDestination = new("UseDevelopmentStorage=true", "verification");

    [Fact]
    public void CheckDestination_TestsVerificationContainer_WithTheKeyVaultsKey_IsAllowed() =>
        AnonymisationStorage.CheckDestination(_productionSource, _testDestination, keyFromKeyVault: true);

    [Fact]
    public void CheckDestination_Azurite_WithAKeyFromConfiguration_IsAllowed() =>
        AnonymisationStorage.CheckDestination(_azuriteSource, _azuriteDestination, keyFromKeyVault: false);

    [Fact]
    public void CheckDestination_RealFiles_WithAKeyFromConfiguration_Throws() =>
        Assert.Throws<InvalidOperationException>(() => AnonymisationStorage.CheckDestination(_productionSource, _testDestination, keyFromKeyVault: false));

    [Fact]
    public void CheckDestination_ProductionsIngestAccount_Throws() =>
        Assert.Throws<InvalidOperationException>(() => AnonymisationStorage.CheckDestination(
            _productionSource,
            new BlobContainerClient(new Uri("https://s189p01giappdingestsa.blob.core.windows.net/verification?sig=x")),
            keyFromKeyVault: true));

    [Fact]
    public void CheckDestination_AnotherContainer_Throws() =>
        Assert.Throws<InvalidOperationException>(() => AnonymisationStorage.CheckDestination(
            _productionSource,
            new BlobContainerClient(new Uri("https://s189t01giaptsingestsa.blob.core.windows.net/ingest?sig=x")),
            keyFromKeyVault: true));

    [Fact]
    public void CheckDestination_TheSourceAccount_Throws() =>
        Assert.Throws<InvalidOperationException>(() => AnonymisationStorage.CheckDestination(
            _productionSource,
            new BlobContainerClient(new Uri("https://giaptrxprod.blob.core.windows.net/verification?sig=x")),
            keyFromKeyVault: true));
}
