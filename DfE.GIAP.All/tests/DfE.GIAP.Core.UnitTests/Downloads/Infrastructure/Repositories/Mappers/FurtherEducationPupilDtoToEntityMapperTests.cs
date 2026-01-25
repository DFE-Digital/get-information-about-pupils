using DfE.GIAP.Core.Common.Infrastructure.CosmosDb.DataTransferObjects;
using DfE.GIAP.Core.Common.Infrastructure.CosmosDb.DataTransferObjects.Entries;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.Entries;
using DfE.GIAP.Core.Downloads.Infrastructure.Repositories.Mappers;

namespace DfE.GIAP.Core.UnitTests.Downloads.Infrastructure.Repositories.Mappers;

public sealed class FurtherEducationPupilDtoToEntityMapperTests
{
    [Fact]
    public void Map_ThrowsArgumentNullException_WhenInputIsNull()
    {
        // Arrange
        FurtherEducationPupilDtoToEntityMapper mapper = new();

        // Act
        Action act = () => mapper.Map(null!);

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void Map_MapsSimplePropertiesCorrectly()
    {
        FurtherEducationPupilDtoToEntityMapper mapper = new();
        FurtherEducationPupilDto dto = new()
        {
            UniqueLearnerNumber = "1234567890",
            Forename = "Alice",
            Surname = "Smith",
            Sex = "F",
            DOB = new DateTime(2005, 5, 1),
            ConcatenatedName = "Smith, Alice",
            PupilPremium = [],
            specialEducationalNeeds = []
        };

        FurtherEducationPupil result = mapper.Map(dto);

        Assert.Equal("1234567890", result.UniqueLearnerNumber);
        Assert.Equal("Alice", result.Forename);
        Assert.Equal("Smith", result.Surname);
        Assert.Equal("F", result.Sex);
        Assert.Equal(new DateTime(2005, 5, 1), result.DOB);
        Assert.Equal("Smith, Alice", result.ConcatenatedName);
    }

    [Fact]
    public void Map_MapsPupilPremiumEntriesCorrectly()
    {
        FurtherEducationPupilDtoToEntityMapper mapper = new();
        FurtherEducationPupilDto dto = new()
        {
            PupilPremium =
            [
                new FurtherEducationPupilPremiumEntryDto
                {
                    NationalCurriculumYear = "Y12",
                    FullTimeEquivalent = "0.8",
                    AcademicYear = "2023/2024"
                }
            ],
            specialEducationalNeeds = []
        };

        FurtherEducationPupil result = mapper.Map(dto);

        Assert.NotNull(result.PupilPremium);
        Assert.Single(result.PupilPremium);

        FurtherEducationPupilPremiumEntry entry = result.PupilPremium[0];
        Assert.Equal("Y12", entry.NationalCurriculumYear);
        Assert.Equal("0.8", entry.FullTimeEquivalent);
        Assert.Equal("2023/2024", entry.AcademicYear);
    }

    [Fact]
    public void Map_MapsSpecialEducationalNeedsEntriesCorrectly()
    {
        FurtherEducationPupilDtoToEntityMapper mapper = new();
        FurtherEducationPupilDto dto = new()
        {
            PupilPremium = [],
            specialEducationalNeeds =
            [
                new SpecialEducationalNeedsEntryDto
                {
                    NationalCurriculumYear = "Y10",
                    Provision = "Support",
                    AcademicYear = "2022/2023"
                }
            ]
        };

        FurtherEducationPupil result = mapper.Map(dto);

        Assert.NotNull(result.specialEducationalNeeds);
        Assert.Single(result.specialEducationalNeeds);

        SpecialEducationalNeedsEntry entry = result.specialEducationalNeeds[0];
        Assert.Equal("Y10", entry.NationalCurriculumYear);
        Assert.Equal("Support", entry.Provision);
        Assert.Equal("2022/2023", entry.AcademicYear);
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
