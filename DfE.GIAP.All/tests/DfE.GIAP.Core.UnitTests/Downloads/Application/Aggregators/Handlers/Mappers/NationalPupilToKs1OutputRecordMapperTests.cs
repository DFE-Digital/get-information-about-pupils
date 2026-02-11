using System.Globalization;
using DfE.GIAP.Core.Common.Application.Helpers;
using DfE.GIAP.Core.Downloads.Application.DataDownloads.Aggregators.Handlers.Mappers;
using DfE.GIAP.Core.Downloads.Application.DataDownloads.DownloadOutputs;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.Entries;
using DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.Downloads.Application.Aggregators.Handlers.Mappers;

public sealed class NationalPupilToKs1OutputRecordMapperTests
{
    [Fact]
    public void Map_ThrowsArgumentNullException_WhenInputIsNull()
    {
        NationalPupilToKs1OutputRecordMapper mapper =
            new NationalPupilToKs1OutputRecordMapper();

        Action act = () => mapper.Map(null!);

        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void Map_ReturnsEmpty_WhenKeyStage1ListIsNull()
    {
        NationalPupilToKs1OutputRecordMapper mapper =
            new NationalPupilToKs1OutputRecordMapper();

        NationalPupil pupil = NationalPupilTestDoubles.Create(includeKeyStages: false);
        pupil.KeyStage1 = null!;

        IEnumerable<KS1OutputRecord> result = mapper.Map(pupil);

        Assert.Empty(result);
    }

    [Fact]
    public void Map_ReturnsEmpty_WhenKeyStage1ListIsEmpty()
    {
        NationalPupilToKs1OutputRecordMapper mapper =
            new NationalPupilToKs1OutputRecordMapper();

        NationalPupil pupil = NationalPupilTestDoubles.Create(includeKeyStages: false);

        IEnumerable<KS1OutputRecord> result = mapper.Map(pupil);

        Assert.Empty(result);
    }

    [Fact]
    public void Map_MapsAllKeyStage1Entries()
    {
        NationalPupilToKs1OutputRecordMapper mapper =
            new NationalPupilToKs1OutputRecordMapper();

        NationalPupil pupil = NationalPupilTestDoubles.Create(includeKeyStages: true);

        IEnumerable<KS1OutputRecord> result = mapper.Map(pupil);

        Assert.Equal(pupil.KeyStage1!.Count, result.Count());
    }

    [Fact]
    public void Map_MapsKeyStage1EntryFieldsCorrectly()
    {
        NationalPupilToKs1OutputRecordMapper mapper =
            new NationalPupilToKs1OutputRecordMapper();

        NationalPupil pupil = NationalPupilTestDoubles.Create(includeKeyStages: true);

        KeyStage1Entry entry = pupil.KeyStage1!.First();

        KS1OutputRecord mapped = mapper.Map(pupil).First();

        Assert.Equal(entry.ACADYR, mapped.ACADYR);
        Assert.Equal(entry.PUPILMATCHINGREF, mapped.PUPILMATCHINGREF);
        Assert.Equal(entry.KS1_ID, mapped.KS1_ID);
        Assert.Equal(entry.UPN, mapped.UPN);
        Assert.Equal(entry.SURNAME, mapped.SURNAME);
        Assert.Equal(entry.FORENAMES, mapped.FORENAMES);
        Assert.Equal(entry.DOB?.ToString(DateFormatting.StandardDateFormat, CultureInfo.InvariantCulture), mapped.DOB);
        Assert.Equal(entry.GENDER, mapped.GENDER);
        Assert.Equal(entry.SEX, mapped.SEX);
        Assert.Equal(entry.LA, mapped.LA);
        Assert.Equal(entry.LA_9Code, mapped.LA_9Code);
        Assert.Equal(entry.ESTAB, mapped.ESTAB);
        Assert.Equal(entry.LAESTAB, mapped.LAESTAB);
        Assert.Equal(entry.URN, mapped.URN);
        Assert.Equal(entry.ToE_CODE, mapped.ToE_CODE);
        Assert.Equal(entry.MMSCH, mapped.MMSCH);
        Assert.Equal(entry.MMSCH2, mapped.MMSCH2);
        Assert.Equal(entry.MSCH, mapped.MSCH);
        Assert.Equal(entry.MSCH2, mapped.MSCH2);
        Assert.Equal(entry.MOB1, mapped.MOB1);
        Assert.Equal(entry.MOB2, mapped.MOB2);
        Assert.Equal(entry.DISC_READ, mapped.DISC_READ);
        Assert.Equal(entry.DISC_WRIT, mapped.DISC_WRIT);
        Assert.Equal(entry.DISC_MAT, mapped.DISC_MAT);
        Assert.Equal(entry.DISC_SCI, mapped.DISC_SCI);
        Assert.Equal(entry.READ_OUTCOME, mapped.READ_OUTCOME);
        Assert.Equal(entry.WRIT_OUTCOME, mapped.WRIT_OUTCOME);
        Assert.Equal(entry.MATH_OUTCOME, mapped.MATH_OUTCOME);
        Assert.Equal(entry.SCI_OUTCOME, mapped.SCI_OUTCOME);
        Assert.Equal(entry.NPDPUB, mapped.NPDPUB);
        Assert.Equal(entry.VERSION, mapped.VERSION);
    }

    [Fact]
    public void Map_HandlesNullEntriesInsideList()
    {
        NationalPupilToKs1OutputRecordMapper mapper =
            new NationalPupilToKs1OutputRecordMapper();

        NationalPupil pupil = NationalPupilTestDoubles.Create(includeKeyStages: true);

        pupil.KeyStage1!.Insert(0, null!);

        List<KS1OutputRecord> result = mapper.Map(pupil).ToList();

        Assert.Equal(pupil.KeyStage1.Count, result.Count);
        Assert.Null(result[0].PUPILMATCHINGREF);
    }

    [Fact]
    public void Map_IgnoresUnmappedFieldsInKeyStage1Entry()
    {
        // These exist on the entry but not on the output
        Assert.Null(typeof(KS1OutputRecord).GetProperty("DISC_PSENG"));
        Assert.Null(typeof(KS1OutputRecord).GetProperty("APS"));
        Assert.Null(typeof(KS1OutputRecord).GetProperty("SCIEXPINVEST"));
        Assert.Null(typeof(KS1OutputRecord).GetProperty("NFTYPE"));
    }
}
