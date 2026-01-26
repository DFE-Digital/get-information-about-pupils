using DfE.GIAP.Core.Downloads.Application.Aggregators.Handlers.Mappers;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.DownloadOutputs;
using DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

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
        FurtherEducationPupil pupil = FurtherEducationPupilTestDoubles.Create();

        // Act
        FurtherEducationPPOutputRecord result = mapper.Map(pupil);

        // Assert
        Assert.Equal(pupil.UniqueLearnerNumber, result.ULN);
        Assert.Equal(pupil.Forename, result.Forename);
        Assert.Equal(pupil.Surname, result.Surname);
        Assert.Equal(pupil.Sex, result.Sex);
        Assert.Equal(pupil.DOB.ToShortDateString(), result.DOB); // ToShortDateString()
    }

    [Fact]
    public void Map_MapsPupilPremiumEntry_WhenEntryExists()
    {
        // Arrange
        FurtherEducationPupilToPpOutputRecordMapper mapper = new();
        FurtherEducationPupil pupil = FurtherEducationPupilTestDoubles.Create(
            includePupilPremium: true);

        // Act
        FurtherEducationPPOutputRecord result = mapper.Map(pupil);

        // Assert
        Assert.Equal(pupil.PupilPremium![0].AcademicYear, result.ACAD_YEAR);
        Assert.Equal(pupil.PupilPremium![0].NationalCurriculumYear, result.NCYear);
        Assert.Equal(pupil.PupilPremium![0].FullTimeEquivalent, result.Pupil_Premium_FTE);
    }

    [Fact]
    public void Map_SetsPpFieldsToNull_WhenNoPpEntryExists()
    {
        // Arrange
        FurtherEducationPupilToPpOutputRecordMapper mapper = new();
        FurtherEducationPupil pupil = FurtherEducationPupilTestDoubles.Create();

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
        FurtherEducationPupil pupil = FurtherEducationPupilTestDoubles.Create();

        // Act
        FurtherEducationPPOutputRecord result = mapper.Map(pupil);

        // Assert
        Assert.Null(result.ACAD_YEAR);
        Assert.Null(result.NCYear);
        Assert.Null(result.Pupil_Premium_FTE);
    }
}

