using DfE.GIAP.Core.PreparedDownloads.Application.Enums;
using DfE.GIAP.Core.PreparedDownloads.Application.FolderPath;

namespace DfE.GIAP.Core.UnitTests.PreparedDownloads.Application.FolderPath;
public sealed class BlobStoragePathContextTests
{
    [Theory]
    [InlineData(OrganisationScope.Establishment, null, null, "123456", "School/123456/")]
    [InlineData(OrganisationScope.LocalAuthority, null, "654321", null, "LA/654321/")]
    [InlineData(OrganisationScope.MultiAcademyTrust, "MAT001", null, null, "MAT/MAT001/")]
    [InlineData(OrganisationScope.SingleAcademyTrust, "SAT001", null, null, "SAT/SAT001/")]
    [InlineData(OrganisationScope.AllUsers, null, null, null, "AllUsers/Metadata/")]
    public void Path_Returns_ExpectedValue(
        OrganisationScope scope,
        string? uniqueIdentifier,
        string? localAuthorityNumber,
        string? uniqueReferenceNumber,
        string expectedPath)
    {
        // Arrange
        BlobStoragePathContext context = BlobStoragePathContext.Create(
            organisationScope: scope,
            uniqueIdentifier: uniqueIdentifier,
            localAuthorityNumber: localAuthorityNumber,
            uniqueReferenceNumber: uniqueReferenceNumber);

        // Act
        string actualPath = context.Path;

        // Assert
        Assert.Equal(expectedPath, actualPath);
    }

    [Theory]
    [InlineData(OrganisationScope.Establishment)]
    [InlineData(OrganisationScope.LocalAuthority)]
    [InlineData(OrganisationScope.MultiAcademyTrust)]
    [InlineData(OrganisationScope.SingleAcademyTrust)]
    public void Create_Throws_ArgumentNullException_WhenRequiredFieldIsMissing(OrganisationScope scope)
    {
        // Act & Assert
        ArgumentNullException ex = Assert.Throws<ArgumentNullException>(() =>
        {
            BlobStoragePathContext.Create(scope);
        });

        Assert.Contains("Value cannot be null", ex.Message);
    }

    [Fact]
    public void Create_ThrowsNotImplementedException_ForUnhandledScope()
    {
        // Arrange
        OrganisationScope invalidScope = (OrganisationScope)999;

        // Act & Assert
        NotImplementedException ex = Assert.Throws<NotImplementedException>(() =>
        {
            BlobStoragePathContext.Create(invalidScope);
        });

        Assert.Contains("Unhandled OrganisationScope", ex.Message);
    }

    [Theory]
    [InlineData(OrganisationScope.Establishment, null, null, "123456", "School/123456/")]
    [InlineData(OrganisationScope.LocalAuthority, null, "654321", null, "LA/654321/")]
    [InlineData(OrganisationScope.MultiAcademyTrust, "MAT001", null, null, "MAT/MAT001/")]
    [InlineData(OrganisationScope.SingleAcademyTrust, "SAT001", null, null, "SAT/SAT001/")]
    [InlineData(OrganisationScope.AllUsers, null, null, null, "AllUsers/Metadata/")]
    public void ResolvePath_ReturnsCorrectPath(
        OrganisationScope scope,
        string? uniqueIdentifier,
        string? localAuthorityNumber,
        string? uniqueReferenceNumber,
        string expectedPath)
    {
        // Arrange
        BlobStoragePathContext context = BlobStoragePathContext.Create(
            organisationScope: scope,
            uniqueIdentifier: uniqueIdentifier,
            localAuthorityNumber: localAuthorityNumber,
            uniqueReferenceNumber: uniqueReferenceNumber);

        // Act
        string resolvedPath = context.ResolvePath();

        // Assert
        Assert.Equal(expectedPath, resolvedPath);
    }
}
