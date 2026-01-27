using DfE.GIAP.Core.Common.Infrastructure.CosmosDb.DataTransferObjects;
using DfE.GIAP.Core.Common.Infrastructure.CosmosDb.DataTransferObjects.Entries;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.Entries;
using DfE.GIAP.Core.Downloads.Infrastructure.Repositories.Mappers;
using DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

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
        PupilPremiumPupilDto dto = PupilPremiumPupilDtoTestDoubles.Generate(1).First();

        PupilPremiumPupil result = mapper.Map(dto);

        Assert.Equal(dto.UniquePupilNumber, result.UniquePupilNumber);
        Assert.Equal(dto.UniqueReferenceNumber, result.UniqueReferenceNumber);
        Assert.Equal(dto.Forename, result.Forename);
        Assert.Equal(dto.Surname, result.Surname);
        Assert.Equal(dto.DOB, result.DOB);
        Assert.Equal(dto.ConcatenatedName, result.ConcatenatedName);
        Assert.Equal(dto.Sex, result.Sex);
    }

    [Fact]
    public void Map_MapsPupilPremiumEntriesCorrectly()
    {
        PupilPremiumDtoToEntityMapper mapper = new();
        PupilPremiumPupilDto dto = PupilPremiumPupilDtoTestDoubles.Generate(1).First();
        PupilPremiumEntryDto dtoEntry = dto.PupilPremium.First();

        PupilPremiumPupil result = mapper.Map(dto);
        PupilPremiumEntry entry = result.PupilPremium[0];

        Assert.NotEmpty(result.PupilPremium);

        Assert.Equal(dtoEntry.UniquePupilNumber, entry.UniquePupilNumber);
        Assert.Equal(dtoEntry.Surname, entry.Surname);
        Assert.Equal(dtoEntry.Forename, entry.Forename);
        Assert.Equal(dtoEntry.Sex, entry.Sex);
        Assert.Equal(dtoEntry.DOB, entry.DOB);
        Assert.Equal(dtoEntry.NCYear, entry.NCYear);

        // ints copied directly
        Assert.Equal(dtoEntry.DeprivationPupilPremium, entry.DeprivationPupilPremium);
        Assert.Equal(dtoEntry.ServiceChildPremium, entry.ServiceChildPremium);
        Assert.Equal(dtoEntry.AdoptedfromCarePremium, entry.AdoptedfromCarePremium);
        Assert.Equal(dtoEntry.LookedAfterPremium, entry.LookedAfterPremium);

        // strings copied directly
        Assert.Equal(dtoEntry.PupilPremiumFTE, entry.PupilPremiumFTE);
        Assert.Equal(dtoEntry.PupilPremiumCashAmount, entry.PupilPremiumCashAmount);
        Assert.Equal(dtoEntry.PupilPremiumFYStartDate, entry.PupilPremiumFYStartDate);
        Assert.Equal(dtoEntry.PupilPremiumFYEndDate, entry.PupilPremiumFYEndDate);
        Assert.Equal(dtoEntry.LastFSM, entry.LastFSM);
        Assert.Equal(dtoEntry.MODSERVICE, entry.MODSERVICE);
        Assert.Equal(dtoEntry.CENSUSSERVICEEVER6, entry.CENSUSSERVICEEVER6);
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
