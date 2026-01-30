using DfE.GIAP.Core.Downloads.Application.Aggregators.Handlers.Mappers;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.DownloadOutputs;
using DfE.GIAP.Core.Downloads.Application.Models.Entries;
using DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.Downloads.Application.Aggregators.Handlers.Mappers;

public sealed class NationalPupilToMtcOutputRecordMapperTests
{
    [Fact]
    public void Map_ThrowsArgumentNullException_WhenInputIsNull()
    {
        NationalPupilToMtcOutputRecordMapper mapper =
            new NationalPupilToMtcOutputRecordMapper();

        Action act = () => mapper.Map(null!);

        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void Map_ReturnsEmpty_WhenMtcListIsNull()
    {
        NationalPupilToMtcOutputRecordMapper mapper =
            new NationalPupilToMtcOutputRecordMapper();

        NationalPupil pupil = NationalPupilTestDoubles.Create(includeMtc: false);
        pupil.MTC = null!;

        IEnumerable<MTCOutput> result = mapper.Map(pupil);

        Assert.Empty(result);
    }

    [Fact]
    public void Map_ReturnsEmpty_WhenMtcListIsEmpty()
    {
        NationalPupilToMtcOutputRecordMapper mapper =
            new NationalPupilToMtcOutputRecordMapper();

        NationalPupil pupil = NationalPupilTestDoubles.Create(includeMtc: false);

        IEnumerable<MTCOutput> result = mapper.Map(pupil);

        Assert.Empty(result);
    }

    [Fact]
    public void Map_MapsAllMtcEntries()
    {
        NationalPupilToMtcOutputRecordMapper mapper =
            new NationalPupilToMtcOutputRecordMapper();

        NationalPupil pupil = NationalPupilTestDoubles.Create(includeMtc: true);

        IEnumerable<MTCOutput> result = mapper.Map(pupil);

        Assert.Equal(pupil.MTC!.Count, result.Count());
    }

    [Fact]
    public void Map_MapsMtcEntryFieldsCorrectly()
    {
        NationalPupilToMtcOutputRecordMapper mapper =
            new NationalPupilToMtcOutputRecordMapper();

        NationalPupil pupil = NationalPupilTestDoubles.Create(includeMtc: true);

        MtcEntry entry = pupil.MTC!.First();

        MTCOutput mapped = mapper.Map(pupil).First();

        Assert.Equal(entry.ACADYR, mapped.ACADYR);
        Assert.Equal(entry.PupilMatchingRef, mapped.PupilMatchingRef);
        Assert.Equal(entry.UPN, mapped.UPN);
        Assert.Equal(entry.Surname, mapped.Surname);
        Assert.Equal(entry.Forename, mapped.Forename);
        Assert.Equal(entry.Sex, mapped.Sex);
        Assert.Equal(entry.DOB?.ToShortDateString(), mapped.DOB);
        Assert.Equal(entry.LA, mapped.LA);
        Assert.Equal(entry.LA_9Code, mapped.LA_9Code);
        Assert.Equal(entry.Estab, mapped.ESTAB);
        Assert.Equal(entry.LAEstab, mapped.LAESTAB);
        Assert.Equal(entry.URN, mapped.URN);
        Assert.Equal(entry.ToECode, mapped.ToECode);
        Assert.Equal(entry.FormMark, mapped.FormMark);
        Assert.Equal(entry.PupilStatus, mapped.PupilStatus);
        Assert.Equal(entry.ReasonNotTakingCheck, mapped.ReasonNotTakingCheck);
    }

    [Fact]
    public void Map_HandlesNullEntriesInsideList()
    {
        NationalPupilToMtcOutputRecordMapper mapper =
            new NationalPupilToMtcOutputRecordMapper();

        NationalPupil pupil = NationalPupilTestDoubles.Create(includeMtc: true);

        pupil.MTC!.Insert(0, null!);

        List<MTCOutput> result = mapper.Map(pupil).ToList();

        Assert.Equal(pupil.MTC.Count, result.Count);
        Assert.Null(result[0].PupilMatchingRef);
    }

    [Fact]
    public void Map_IgnoresUnmappedFieldsInMtcEntry()
    {
        // These exist on the entry but not on the output
        Assert.Null(typeof(MTCOutput).GetProperty("SomeUnmappedField"));
        Assert.Null(typeof(MTCOutput).GetProperty("RandomExtraField"));
    }
}
