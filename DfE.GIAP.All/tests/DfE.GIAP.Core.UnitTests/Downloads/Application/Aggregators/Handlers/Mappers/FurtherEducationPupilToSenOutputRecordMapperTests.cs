using DfE.GIAP.Core.Downloads.Application.Aggregators.Handlers.Mappers;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.DownloadOutputs;
using DfE.GIAP.Core.Downloads.Application.Models.Entries;

namespace DfE.GIAP.Core.UnitTests.Downloads.Application.Aggregators.Handlers.Mappers;

public sealed class FurtherEducationPupilToSenOutputRecordMapperTests
{
    [Fact]
    public void Map_ThrowsArgumentNullException_WhenInputIsNull()
    {
        // Arrange
        FurtherEducationPupilToSenOutputRecordMapper mapper = new();

        // Act
        Action act = () => mapper.Map(null!);

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void Map_MapsSimplePropertiesCorrectly()
    {
        // Arrange
        FurtherEducationPupilToSenOutputRecordMapper mapper = new();
        FurtherEducationPupil pupil = new()
        {
            UniqueLearnerNumber = "1234567890",
            Forename = "Alice",
            Surname = "Smith",
            Sex = "F",
            DOB = new DateTime(2005, 3, 15),
            specialEducationalNeeds = []
        };

        // Act
        FurtherEducationSENOutputRecord result = mapper.Map(pupil);

        // Assert
        Assert.Equal("1234567890", result.ULN);
        Assert.Equal("Alice", result.Forename);
        Assert.Equal("Smith", result.Surname);
        Assert.Equal("F", result.Sex);
        Assert.Equal("15/03/2005", result.DOB); // ToShortDateString()
    }

    [Fact]
    public void Map_MapsSenEntry_WhenEntryExists()
    {
        // Arrange
        FurtherEducationPupilToSenOutputRecordMapper mapper = new();

        SpecialEducationalNeedsEntry senEntry = new()
        {
            NationalCurriculumYear = "Y10",
            AcademicYear = "2022/2023",
            Provision = "Support"
        };

        FurtherEducationPupil pupil = new()
        {
            specialEducationalNeeds = [senEntry]
        };

        // Act
        FurtherEducationSENOutputRecord result = mapper.Map(pupil);

        // Assert
        Assert.Equal("Y10", result.NCYear);
        Assert.Equal("2022/2023", result.ACAD_YEAR);
        Assert.Equal("Support", result.SEN_PROVISION);
    }

    [Fact]
    public void Map_SetsSenFieldsToNull_WhenNoSenEntryExists()
    {
        // Arrange
        FurtherEducationPupilToSenOutputRecordMapper mapper = new();
        FurtherEducationPupil pupil = new()
        {
            specialEducationalNeeds = [] // empty list
        };

        // Act
        FurtherEducationSENOutputRecord result = mapper.Map(pupil);

        // Assert
        Assert.Null(result.NCYear);
        Assert.Null(result.ACAD_YEAR);
        Assert.Null(result.SEN_PROVISION);
    }

    [Fact]
    public void Map_SetsSenFieldsToNull_WhenSenListIsNull()
    {
        // Arrange
        FurtherEducationPupilToSenOutputRecordMapper mapper = new();
        FurtherEducationPupil pupil = new()
        {
            specialEducationalNeeds = null
        };

        // Act
        FurtherEducationSENOutputRecord result = mapper.Map(pupil);

        // Assert
        Assert.Null(result.NCYear);
        Assert.Null(result.ACAD_YEAR);
        Assert.Null(result.SEN_PROVISION);
    }
}
