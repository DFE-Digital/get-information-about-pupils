using DfE.GIAP.Core.Downloads.Application.Aggregators.Handlers.Mappers;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.DownloadOutputs;
using DfE.GIAP.Core.Downloads.Application.Models.Entries;

namespace DfE.GIAP.Core.UnitTests.Downloads.Application.Aggregators.Handlers.Mappers;

public sealed class FurtherEducationPupilToPpOutputRecordMapperTests
{
    [Fact]
    public void Map_ThrowsArgumentNullException_WhenInputIsNull()
    {
        // Arrange
        FurtherEducationPupilToPpOutputRecordMapper mapper = new();

        // Act
        Action act = () => mapper.Map(null!);

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void Map_MapsSimplePropertiesCorrectly()
    {
        // Arrange
        FurtherEducationPupilToPpOutputRecordMapper mapper = new();
        FurtherEducationPupil pupil = new()
        {
            UniqueLearnerNumber = "1234567890",
            Forename = "Alice",
            Surname = "Smith",
            Sex = "F",
            DOB = new DateTime(2005, 3, 15),
            PupilPremium = []
        };

        // Act
        FurtherEducationPPOutputRecord result = mapper.Map(pupil);

        // Assert
        Assert.Equal("1234567890", result.ULN);
        Assert.Equal("Alice", result.Forename);
        Assert.Equal("Smith", result.Surname);
        Assert.Equal("F", result.Sex);
        Assert.Equal("15/03/2005", result.DOB); // ToShortDateString()
    }

    [Fact]
    public void Map_MapsPupilPremiumEntry_WhenEntryExists()
    {
        // Arrange
        FurtherEducationPupilToPpOutputRecordMapper mapper = new();

        FurtherEducationPupilPremiumEntry ppEntry = new()
        {
            AcademicYear = "2023/2024",
            NationalCurriculumYear = "Y12",
            FullTimeEquivalent = "0.8"
        };

        FurtherEducationPupil pupil = new()
        {
            PupilPremium = [ppEntry]
        };

        // Act
        FurtherEducationPPOutputRecord result = mapper.Map(pupil);

        // Assert
        Assert.Equal("2023/2024", result.ACAD_YEAR);
        Assert.Equal("Y12", result.NCYear);
        Assert.Equal("0.8", result.Pupil_Premium_FTE);
    }

    [Fact]
    public void Map_SetsPpFieldsToNull_WhenNoPpEntryExists()
    {
        // Arrange
        FurtherEducationPupilToPpOutputRecordMapper mapper = new();
        FurtherEducationPupil pupil = new()
        {
            PupilPremium = [] // empty list
        };

        // Act
        FurtherEducationPPOutputRecord result = mapper.Map(pupil);

        // Assert
        Assert.Null(result.ACAD_YEAR);
        Assert.Null(result.NCYear);
        Assert.Null(result.Pupil_Premium_FTE);
    }

    [Fact]
    public void Map_SetsPpFieldsToNull_WhenPpListIsNull()
    {
        // Arrange
        FurtherEducationPupilToPpOutputRecordMapper mapper = new();
        FurtherEducationPupil pupil = new()
        {
            PupilPremium = null
        };

        // Act
        FurtherEducationPPOutputRecord result = mapper.Map(pupil);

        // Assert
        Assert.Null(result.ACAD_YEAR);
        Assert.Null(result.NCYear);
        Assert.Null(result.Pupil_Premium_FTE);
    }
}

