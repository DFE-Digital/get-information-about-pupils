using DfE.GIAP.Core.Downloads.Application.Aggregators.Handlers.Mappers;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.DownloadOutputs;
using DfE.GIAP.Core.Downloads.Application.Models.Entries;
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
    public void Map_ReturnsEmpty_WhenSenListIsNull()
    {
        // Arrange
        FurtherEducationPupilToSenOutputRecordMapper mapper = new();
        FurtherEducationPupil pupil = FurtherEducationPupilTestDoubles.Create(includeSen: false);
        pupil.specialEducationalNeeds = null;

        // Act
        IEnumerable<FurtherEducationSENOutputRecord> result = mapper.Map(pupil);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Map_ReturnsEmpty_WhenSenListIsEmpty()
    {
        // Arrange
        FurtherEducationPupilToSenOutputRecordMapper mapper = new();
        FurtherEducationPupil pupil = FurtherEducationPupilTestDoubles.Create(includeSen: false);
        pupil.specialEducationalNeeds = new List<SpecialEducationalNeedsEntry>();

        // Act
        IEnumerable<FurtherEducationSENOutputRecord> result = mapper.Map(pupil);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Map_MapsSimpleProperties_ForEachEntry()
    {
        // Arrange
        FurtherEducationPupilToSenOutputRecordMapper mapper = new();
        FurtherEducationPupil pupil = FurtherEducationPupilTestDoubles.Create(includeSen: true);

        // Act
        IEnumerable<FurtherEducationSENOutputRecord> result = mapper.Map(pupil);

        // Assert
        FurtherEducationSENOutputRecord first = result.First();

        Assert.Equal(pupil.UniqueLearnerNumber, first.ULN);
        Assert.Equal(pupil.Forename, first.Forename);
        Assert.Equal(pupil.Surname, first.Surname);
        Assert.Equal(pupil.Sex, first.Sex);
        Assert.Equal(pupil.DOB.ToShortDateString(), first.DOB);
    }

    [Fact]
    public void Map_MapsAllSenEntries()
    {
        // Arrange
        FurtherEducationPupilToSenOutputRecordMapper mapper = new();
        FurtherEducationPupil pupil = FurtherEducationPupilTestDoubles.Create(includeSen: true);

        // Act
        IEnumerable<FurtherEducationSENOutputRecord> result = mapper.Map(pupil);

        // Assert
        Assert.NotNull(pupil.specialEducationalNeeds);
        Assert.Equal(pupil.specialEducationalNeeds.Count, result.Count());
    }

    [Fact]
    public void Map_MapsSenEntryFieldsCorrectly()
    {
        // Arrange
        FurtherEducationPupilToSenOutputRecordMapper mapper = new();
        FurtherEducationPupil pupil = FurtherEducationPupilTestDoubles.Create(includeSen: true);

        SpecialEducationalNeedsEntry entry = pupil.specialEducationalNeeds!.First();

        // Act
        IEnumerable<FurtherEducationSENOutputRecord> result = mapper.Map(pupil);
        FurtherEducationSENOutputRecord mapped = result.First();

        // Assert
        Assert.Equal(entry.NationalCurriculumYear, mapped.NCYear);
        Assert.Equal(entry.AcademicYear, mapped.ACAD_YEAR);
        Assert.Equal(entry.Provision, mapped.SEN_PROVISION);
    }
}
