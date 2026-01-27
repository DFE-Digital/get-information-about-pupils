using DfE.GIAP.Core.Common.Infrastructure.CosmosDb.DataTransferObjects;
using DfE.GIAP.Core.Common.Infrastructure.CosmosDb.DataTransferObjects.Entries;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.Entries;
using DfE.GIAP.Core.Downloads.Infrastructure.Repositories.Mappers;

namespace DfE.GIAP.Core.UnitTests.Downloads.Infrastructure.Repositories.Mappers;

public sealed class PupilPremiumDtoToEntityMapperTests
{
    [Fact]
    public void Map_ThrowsArgumentNullException_WhenInputIsNull()
    {
        PupilPremiumDtoToEntityMapper mapper = new();
        Assert.Throws<ArgumentNullException>(() => mapper.Map(null!));
    }

    [Fact]
    public void Map_MapsSimplePropertiesCorrectly()
    {
        PupilPremiumDtoToEntityMapper mapper = new();
        PupilPremiumPupilDto dto = new()
        {
            UniquePupilNumber = "UPN123",
            UniqueReferenceNumber = "URN456",
            Forename = "Alice",
            Surname = "Smith",
            DOB = new DateTime(2010, 4, 1),
            ConcatenatedName = "Smith, Alice",
            Sex = "F",
            PupilPremium = []
        };

        PupilPremiumPupil result = mapper.Map(dto);

        Assert.Equal("UPN123", result.UniquePupilNumber);
        Assert.Equal("URN456", result.UniqueReferenceNumber);
        Assert.Equal("Alice", result.Forename);
        Assert.Equal("Smith", result.Surname);
        Assert.Equal(new DateTime(2010, 4, 1), result.DOB);
        Assert.Equal("Smith, Alice", result.ConcatenatedName);
        Assert.Equal("F", result.Sex);
    }

    [Fact]
    public void Map_MapsPupilPremiumEntriesCorrectly()
    {
        PupilPremiumDtoToEntityMapper mapper = new();
        PupilPremiumPupilDto dto = new()
        {
            PupilPremium =
            [
                new PupilPremiumEntryDto
                {
                    UniquePupilNumber = "UPN1",
                    Surname = "Brown",
                    Forename = "Tom",
                    Sex = "M",
                    DOB = "2011-02-02",
                    NCYear = "Y5",
                    DeprivationPupilPremium = 1,
                    ServiceChildPremium = 0,
                    AdoptedfromCarePremium = 1,
                    LookedAfterPremium = 0,
                    PupilPremiumFTE = "0.75",
                    PupilPremiumCashAmount = "350",
                    PupilPremiumFYStartDate = "2023-04-01",
                    PupilPremiumFYEndDate = "2024-03-31",
                    LastFSM = "2022-10-01",
                    MODSERVICE = "0",
                    CENSUSSERVICEEVER6 = "1"
                }
            ]
        };

        PupilPremiumPupil result = mapper.Map(dto);

        Assert.Single(result.PupilPremium);

        PupilPremiumEntry entry = result.PupilPremium[0];

        // Strings copied directly
        Assert.Equal("UPN1", entry.UniquePupilNumber);
        Assert.Equal("Brown", entry.Surname);
        Assert.Equal("Tom", entry.Forename);
        Assert.Equal("M", entry.Sex);
        Assert.Equal("2011-02-02", entry.DOB);
        Assert.Equal("Y5", entry.NCYear);

        // ints copied directly
        Assert.Equal(1, entry.DeprivationPupilPremium);
        Assert.Equal(0, entry.ServiceChildPremium);
        Assert.Equal(1, entry.AdoptedfromCarePremium);
        Assert.Equal(0, entry.LookedAfterPremium);

        // strings copied directly
        Assert.Equal("0.75", entry.PupilPremiumFTE);
        Assert.Equal("350", entry.PupilPremiumCashAmount);
        Assert.Equal("2023-04-01", entry.PupilPremiumFYStartDate);
        Assert.Equal("2024-03-31", entry.PupilPremiumFYEndDate);
        Assert.Equal("2022-10-01", entry.LastFSM);
        Assert.Equal("0", entry.MODSERVICE);
        Assert.Equal("1", entry.CENSUSSERVICEEVER6);
    }

    [Fact]
    public void Map_SetsEmptyList_WhenCollectionIsNull()
    {
        PupilPremiumDtoToEntityMapper mapper = new();
        PupilPremiumPupilDto dto = new();

        PupilPremiumPupil result = mapper.Map(dto);

        Assert.NotNull(result.PupilPremium);
        Assert.Empty(result.PupilPremium);
    }
}
