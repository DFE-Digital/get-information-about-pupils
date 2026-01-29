using DfE.GIAP.Core.Common.Infrastructure.CosmosDb.DataTransferObjects;
using DfE.GIAP.Core.Common.Infrastructure.CosmosDb.DataTransferObjects.Entries;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.Entries;
using DfE.GIAP.Core.Downloads.Infrastructure.Repositories.Mappers;
using DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.Downloads.Infrastructure.Repositories.Mappers;

public sealed class FurtherEducationPupilDtoToEntityMapperTests
{
    [Fact]
    public void Map_ThrowsArgumentNullException_WhenInputIsNull()
    {
        FurtherEducationPupilDtoToEntityMapper mapper = new();

        Action act = () => mapper.Map(null!);

        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void Map_MapsSimplePropertiesCorrectly()
    {
        FurtherEducationPupilDtoToEntityMapper mapper = new();
        FurtherEducationPupilDto dto = FurtherEducationPupilDtoTestDoubles.Generate(count: 1).Single();

        FurtherEducationPupil result = mapper.Map(dto);

        Assert.Equal(dto.UniqueLearnerNumber, result.UniqueLearnerNumber);
        Assert.Equal(dto.Forename, result.Forename);
        Assert.Equal(dto.Surname, result.Surname);
        Assert.Equal(dto.Sex, result.Sex);
        Assert.Equal(dto.DOB, result.DOB);
        Assert.Equal(dto.ConcatenatedName, result.ConcatenatedName);
    }

    [Fact]
    public void Map_MapsPupilPremiumEntriesCorrectly()
    {
        FurtherEducationPupilDtoToEntityMapper mapper = new();
        FurtherEducationPupilDto dto = FurtherEducationPupilDtoTestDoubles.Generate(count: 1).Single();
        FurtherEducationPupilPremiumEntryDto dtoPpEntry = dto.PupilPremium.First();

        FurtherEducationPupil result = mapper.Map(dto);

        Assert.NotNull(result.PupilPremium);
        Assert.NotEmpty(result.PupilPremium);

        FurtherEducationPupilPremiumEntry entry = result.PupilPremium[0];
        Assert.Equal(dtoPpEntry.NationalCurriculumYear, entry.NationalCurriculumYear);
        Assert.Equal(dtoPpEntry.FullTimeEquivalent, entry.FullTimeEquivalent);
        Assert.Equal(dtoPpEntry.AcademicYear, entry.AcademicYear);
    }

    [Fact]
    public void Map_MapsSpecialEducationalNeedsEntriesCorrectly()
    {
        FurtherEducationPupilDtoToEntityMapper mapper = new();
        FurtherEducationPupilDto dto = FurtherEducationPupilDtoTestDoubles.Generate(count: 1).Single();
        SpecialEducationalNeedsEntryDto dtoSenEntry = dto.specialEducationalNeeds.First();

        FurtherEducationPupil result = mapper.Map(dto);

        Assert.NotNull(result.specialEducationalNeeds);
        Assert.NotEmpty(result.specialEducationalNeeds);

        SpecialEducationalNeedsEntry entry = result.specialEducationalNeeds[0];
        Assert.Equal(dtoSenEntry.NationalCurriculumYear, entry.NationalCurriculumYear);
        Assert.Equal(dtoSenEntry.Provision, entry.Provision);
        Assert.Equal(dtoSenEntry.AcademicYear, entry.AcademicYear);
    }

    [Fact]
    public void Map_SetsEmptyLists_WhenCollectionsAreNull()
    {
        FurtherEducationPupilDtoToEntityMapper mapper = new();
        FurtherEducationPupilDto dto = new();

        FurtherEducationPupil result = mapper.Map(dto);

        Assert.NotNull(result.PupilPremium);
        Assert.Empty(result.PupilPremium);

        Assert.NotNull(result.specialEducationalNeeds);
        Assert.Empty(result.specialEducationalNeeds);
    }
}
