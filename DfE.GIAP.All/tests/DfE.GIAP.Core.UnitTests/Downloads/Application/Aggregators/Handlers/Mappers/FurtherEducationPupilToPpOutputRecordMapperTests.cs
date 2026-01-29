using DfE.GIAP.Core.Downloads.Application.Aggregators.Handlers.Mappers;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.DownloadOutputs;
using DfE.GIAP.Core.Downloads.Application.Models.Entries;
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
    public void Map_ReturnsEmpty_WhenPupilPremiumListIsNull()
    {
        // Arrange
        FurtherEducationPupilToPpOutputRecordMapper mapper = new();
        FurtherEducationPupil pupil = FurtherEducationPupilTestDoubles.Create(includePupilPremium: false);
        pupil.PupilPremium = null;

        // Act
        IEnumerable<FurtherEducationPPOutputRecord> result = mapper.Map(pupil);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Map_ReturnsEmpty_WhenPupilPremiumListIsEmpty()
    {
        // Arrange
        FurtherEducationPupilToPpOutputRecordMapper mapper = new();
        FurtherEducationPupil pupil = FurtherEducationPupilTestDoubles.Create(includePupilPremium: false);
        pupil.PupilPremium = new List<FurtherEducationPupilPremiumEntry>();

        // Act
        IEnumerable<FurtherEducationPPOutputRecord> result = mapper.Map(pupil);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Map_MapsSimpleProperties_ForEachEntry()
    {
        // Arrange
        FurtherEducationPupilToPpOutputRecordMapper mapper = new();
        FurtherEducationPupil pupil = FurtherEducationPupilTestDoubles.Create(includePupilPremium: true);

        // Act
        IEnumerable<FurtherEducationPPOutputRecord> result = mapper.Map(pupil);

        // Assert
        FurtherEducationPPOutputRecord first = result.First();

        Assert.Equal(pupil.UniqueLearnerNumber, first.ULN);
        Assert.Equal(pupil.Forename, first.Forename);
        Assert.Equal(pupil.Surname, first.Surname);
        Assert.Equal(pupil.Sex, first.Sex);
        Assert.Equal(pupil.DOB?.ToShortDateString(), first.DOB);
    }

    [Fact]
    public void Map_MapsAllPupilPremiumEntries()
    {
        // Arrange
        FurtherEducationPupilToPpOutputRecordMapper mapper = new();
        FurtherEducationPupil pupil = FurtherEducationPupilTestDoubles.Create(includePupilPremium: true);

        // Act
        IEnumerable<FurtherEducationPPOutputRecord> result = mapper.Map(pupil);

        // Assert
        Assert.NotNull(pupil.PupilPremium);
        Assert.Equal(pupil.PupilPremium.Count, result.Count());
    }

    [Fact]
    public void Map_MapsPupilPremiumEntryFieldsCorrectly()
    {
        // Arrange
        FurtherEducationPupilToPpOutputRecordMapper mapper = new();
        FurtherEducationPupil pupil = FurtherEducationPupilTestDoubles.Create(includePupilPremium: true);

        FurtherEducationPupilPremiumEntry entry = pupil.PupilPremium!.First();

        // Act
        IEnumerable<FurtherEducationPPOutputRecord> result = mapper.Map(pupil);
        FurtherEducationPPOutputRecord mapped = result.First();

        // Assert
        Assert.Equal(entry.AcademicYear, mapped.ACAD_YEAR);
        Assert.Equal(entry.NationalCurriculumYear, mapped.NCYear);
        Assert.Equal(entry.FullTimeEquivalent, mapped.Pupil_Premium_FTE);
    }
}
