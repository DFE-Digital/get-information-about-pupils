using DfE.GIAP.Web.Features.Auth.Application.Models;
using DfE.GIAP.Web.Features.Auth.Infrastructure.DataTransferObjects;
using DfE.GIAP.Web.Features.Auth.Infrastructure.Mappers;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.Auth.Infrastructure.Mappers;

public class OrganisationDtoToEntityOrganisationTests
{
    [Fact]
    public void Map_ThrowsArgumentNullException_WhenInputIsNull()
    {
        // Arrange
        OrganisationDtoToEntityOrganisation mapper = new();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => mapper.Map(null!));
    }

    [Fact]
    public void Map_MapsBasicPropertiesCorrectly()
    {
        // Arrange
        OrganisationDtoToEntityOrganisation mapper = new();
        OrganisationDto dto = new()
        {
            Id = "org1",
            Name = "Test Org",
            StatutoryLowAge = "5",
            StatutoryHighAge = "16",
            EstablishmentNumber = "12345",
            UniqueReferenceNumber = "URN001",
            UniqueIdentifier = "UID001",
            UKProviderReferenceNumber = "UKPRN001"
        };

        // Act
        Organisation result = mapper.Map(dto);

        // Assert
        Assert.Equal("org1", result.Id);
        Assert.Equal("Test Org", result.Name);
        Assert.Equal("5", result.StatutoryLowAge);
        Assert.Equal("16", result.StatutoryHighAge);
        Assert.Equal("12345", result.EstablishmentNumber);
        Assert.Equal("URN001", result.UniqueReferenceNumber);
        Assert.Equal("UID001", result.UniqueIdentifier);
        Assert.Equal("UKPRN001", result.UKProviderReferenceNumber);
    }

    [Fact]
    public void Map_MapsNestedObjectsCorrectly()
    {
        // Arrange
        OrganisationDtoToEntityOrganisation mapper = new();
        OrganisationDto dto = new()
        {
            Id = "org2",
            Category = new OrganisationCategoryDto { Id = "cat1" },
            EstablishmentType = new EstablishmentTypeDto { Id = "type1" },
            LocalAuthority = new LocalAuthorityDto { Code = "LA001" }
        };

        // Act
        Organisation result = mapper.Map(dto);

        // Assert
        Assert.NotNull(result.Category);
        Assert.Equal("cat1", result.Category!.Id);

        Assert.NotNull(result.EstablishmentType);
        Assert.Equal("type1", result.EstablishmentType!.Id);

        Assert.NotNull(result.LocalAuthority);
        Assert.Equal("LA001", result.LocalAuthority!.Code);
    }

    [Fact]
    public void Map_AllowsNullNestedObjects()
    {
        // Arrange
        OrganisationDtoToEntityOrganisation mapper = new();
        OrganisationDto dto = new()
        {
            Id = "org3",
            Category = null,
            EstablishmentType = null,
            LocalAuthority = null
        };

        // Act
        Organisation result = mapper.Map(dto);

        // Assert
        Assert.Null(result.Category);
        Assert.Null(result.EstablishmentType);
        Assert.Null(result.LocalAuthority);
    }
}
