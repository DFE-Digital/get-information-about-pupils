using DfE.GIAP.Core.Downloads.Application.DataDownloads.Aggregators.Handlers.Mappers;
using DfE.GIAP.Core.Downloads.Application.DataDownloads.DownloadOutputs;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.Entries;
using DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.Downloads.Application.Aggregators.Handlers.Mappers;

public sealed class PupilPremiumPupilToPupilPremiumOutputRecordMapperTests
{
    [Fact]
    public void Map_ThrowsArgumentNullException_WhenInputIsNull()
    {
        PupilPremiumPupilToPupilPremiumOutputRecordMapper mapper = new();

        Action act = () => mapper.Map(null!);

        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void Map_ReturnsEmpty_WhenPupilPremiumListIsNull()
    {
        PupilPremiumPupilToPupilPremiumOutputRecordMapper mapper = new();
        PupilPremiumPupil pupil = PupilPremiumPupilTestDouble.Create(includePupilPremium: false);
        pupil.PupilPremium = null!;

        IEnumerable<PupilPremiumOutputRecord> result = mapper.Map(pupil);

        Assert.Empty(result);
    }

    [Fact]
    public void Map_ReturnsEmpty_WhenPupilPremiumListIsEmpty()
    {
        PupilPremiumPupilToPupilPremiumOutputRecordMapper mapper = new();
        PupilPremiumPupil pupil = PupilPremiumPupilTestDouble.Create(includePupilPremium: false);
        pupil.PupilPremium = new List<PupilPremiumEntry>();

        IEnumerable<PupilPremiumOutputRecord> result = mapper.Map(pupil);

        Assert.Empty(result);
    }

    [Fact]
    public void Map_MapsAllPupilPremiumEntries()
    {
        PupilPremiumPupilToPupilPremiumOutputRecordMapper mapper = new();
        PupilPremiumPupil pupil = PupilPremiumPupilTestDouble.Create(includePupilPremium: true);

        IEnumerable<PupilPremiumOutputRecord> result = mapper.Map(pupil);

        Assert.Equal(pupil.PupilPremium.Count, result.Count());
    }

    [Fact]
    public void Map_MapsPupilPremiumEntryFieldsCorrectly()
    {
        PupilPremiumPupilToPupilPremiumOutputRecordMapper mapper = new();
        PupilPremiumPupil pupil = PupilPremiumPupilTestDouble.Create(includePupilPremium: true);

        PupilPremiumEntry entry = pupil.PupilPremium!.First();

        IEnumerable<PupilPremiumOutputRecord> result = mapper.Map(pupil);
        PupilPremiumOutputRecord mapped = result.First();

        Assert.Equal(entry.NCYear, mapped.NCYear);
        Assert.Equal(entry.DeprivationPupilPremium, mapped.DeprivationPupilPremium);
        Assert.Equal(entry.ServiceChildPremium, mapped.ServiceChildPremium);
        Assert.Equal(entry.AdoptedfromCarePremium, mapped.AdoptedfromCarePremium);
        Assert.Equal(entry.LookedAfterPremium, mapped.LookedAfterPremium);
        Assert.Equal(entry.PupilPremiumFTE, mapped.PupilPremiumFTE);
        Assert.Equal(entry.PupilPremiumCashAmount, mapped.PupilPremiumCashAmount);
        Assert.Equal(entry.PupilPremiumFYStartDate, mapped.PupilPremiumFYStartDate);
        Assert.Equal(entry.PupilPremiumFYEndDate, mapped.PupilPremiumFYEndDate);
        Assert.Equal(entry.LastFSM, mapped.LastFSM);
    }

    [Fact]
    public void Map_IgnoresUnmappedFieldsInPupilPremiumEntry()
    {
        PupilPremiumPupilToPupilPremiumOutputRecordMapper mapper = new();
        PupilPremiumPupil pupil = PupilPremiumPupilTestDouble.Create(includePupilPremium: true);

        IEnumerable<PupilPremiumOutputRecord> result = mapper.Map(pupil);

        // These properties do not exist on the output record
        Assert.Null(typeof(PupilPremiumOutputRecord).GetProperty("MODSERVICE"));
        Assert.Null(typeof(PupilPremiumOutputRecord).GetProperty("CENSUSSERVICEEVER6"));
    }
}
