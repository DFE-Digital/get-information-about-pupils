using DfE.GIAP.Core.Downloads.Application.Aggregators.Handlers.Mappers;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.DownloadOutputs;
using DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

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
        FurtherEducationPupil pupil = FurtherEducationPupilTestDoubles.Create();

        // Act
        FurtherEducationSENOutputRecord result = mapper.Map(pupil);

        // Assert
        Assert.Equal(pupil.UniqueLearnerNumber, result.ULN);
        Assert.Equal(pupil.Forename, result.Forename);
        Assert.Equal(pupil.Surname, result.Surname);
        Assert.Equal(pupil.Sex, result.Sex);
        Assert.Equal(pupil.DOB.ToShortDateString(), result.DOB);
    }

    [Fact]
    public void Map_MapsSenEntry_WhenEntryExists()
    {
        // Arrange
        FurtherEducationPupilToSenOutputRecordMapper mapper = new();
        FurtherEducationPupil pupil = FurtherEducationPupilTestDoubles.Create(
            includeSen: true);

        // Act
        FurtherEducationSENOutputRecord result = mapper.Map(pupil);

        // Assert
        Assert.Equal(pupil.specialEducationalNeeds![0].NationalCurriculumYear, result.NCYear);
        Assert.Equal(pupil.specialEducationalNeeds![0].AcademicYear, result.ACAD_YEAR);
        Assert.Equal(pupil.specialEducationalNeeds![0].Provision, result.SEN_PROVISION);
    }

    [Fact]
    public void Map_SetsSenFieldsToNull_WhenNoSenEntryExists()
    {
        // Arrange
        FurtherEducationPupilToSenOutputRecordMapper mapper = new();
        FurtherEducationPupil pupil = FurtherEducationPupilTestDoubles.Create();

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
        FurtherEducationPupil pupil = FurtherEducationPupilTestDoubles.Create();

        // Act
        FurtherEducationSENOutputRecord result = mapper.Map(pupil);

        // Assert
        Assert.Null(result.NCYear);
        Assert.Null(result.ACAD_YEAR);
        Assert.Null(result.SEN_PROVISION);
    }
}
