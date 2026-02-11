using System.Globalization;
using DfE.GIAP.Core.Common.Application.Helpers;
using DfE.GIAP.Core.Downloads.Application.DataDownloads.Aggregators.Handlers.Mappers;
using DfE.GIAP.Core.Downloads.Application.DataDownloads.DownloadOutputs;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.Entries;
using DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.Downloads.Application.Aggregators.Handlers.Mappers;

public sealed class NationalPupilToKs2OutputRecordMapperTests
{
    [Fact]
    public void Map_ThrowsArgumentNullException_WhenInputIsNull()
    {
        NationalPupilToKs2OutputRecordMapper mapper =
            new NationalPupilToKs2OutputRecordMapper();

        Action act = () => mapper.Map(null!);

        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void Map_ReturnsEmpty_WhenKeyStage2ListIsNull()
    {
        NationalPupilToKs2OutputRecordMapper mapper =
            new NationalPupilToKs2OutputRecordMapper();

        NationalPupil pupil = NationalPupilTestDoubles.Create(includeKeyStages: false);
        pupil.KeyStage2 = null!;

        IEnumerable<KS2OutputRecord> result = mapper.Map(pupil);

        Assert.Empty(result);
    }

    [Fact]
    public void Map_ReturnsEmpty_WhenKeyStage2ListIsEmpty()
    {
        NationalPupilToKs2OutputRecordMapper mapper =
            new NationalPupilToKs2OutputRecordMapper();

        NationalPupil pupil = NationalPupilTestDoubles.Create(includeKeyStages: false);

        IEnumerable<KS2OutputRecord> result = mapper.Map(pupil);

        Assert.Empty(result);
    }

    [Fact]
    public void Map_MapsAllKeyStage2Entries()
    {
        NationalPupilToKs2OutputRecordMapper mapper =
            new NationalPupilToKs2OutputRecordMapper();

        NationalPupil pupil = NationalPupilTestDoubles.Create(includeKeyStages: true);

        IEnumerable<KS2OutputRecord> result = mapper.Map(pupil);

        Assert.Equal(pupil.KeyStage2!.Count, result.Count());
    }

    [Fact]
    public void Map_MapsKeyStage2EntryFieldsCorrectly()
    {
        NationalPupilToKs2OutputRecordMapper mapper =
            new NationalPupilToKs2OutputRecordMapper();

        NationalPupil pupil = NationalPupilTestDoubles.Create(includeKeyStages: true);

        KeyStage2Entry entry = pupil.KeyStage2!.First();

        KS2OutputRecord mapped = mapper.Map(pupil).First();

        // Core identity fields
        Assert.Equal(entry.ACADYR, mapped.ACADYR);
        Assert.Equal(entry.PupilMatchingRef, mapped.PupilMatchingRef);
        Assert.Equal(entry.CANDNO, mapped.CANDNO);
        Assert.Equal(entry.CAND_ID, mapped.CAND_ID);
        Assert.Equal(entry.UPN, mapped.UPN);
        Assert.Equal(entry.FORENAMES, mapped.FORENAMES);
        Assert.Equal(entry.SURNAME, mapped.SURNAME);
        Assert.Equal(entry.DOB?.ToString(DateFormatting.StandardDateFormat, CultureInfo.InvariantCulture), mapped.DOB);
        Assert.Equal(entry.YEARGRP, mapped.YEARGRP);
        Assert.Equal(entry.SEX, mapped.SEX);

        // School identifiers
        Assert.Equal(entry.LA, mapped.LA);
        Assert.Equal(entry.LA_9Code, mapped.LA_9Code);
        Assert.Equal(entry.ESTAB, mapped.ESTAB);
        Assert.Equal(entry.LAESTAB, mapped.LAESTAB);
        Assert.Equal(entry.ToE_CODE, mapped.ToE_CODE);
        Assert.Equal(entry.NFTYPE, mapped.NFTYPE);
        Assert.Equal(entry.MMSCH, mapped.MMSCH);
        Assert.Equal(entry.MMSCH2, mapped.MMSCH2);
        Assert.Equal(entry.MSCH, mapped.MSCH);
        Assert.Equal(entry.MSCH2, mapped.MSCH2);
        Assert.Equal(entry.URN, mapped.URN);
        Assert.Equal(entry.URN_AC, mapped.URN_AC);
        Assert.Equal(entry.OPEN_AC, mapped.OPEN_AC);

        // Outcomes
        Assert.Equal(entry.READOUTCOME, mapped.READOUTCOME);
        Assert.Equal(entry.MATOUTCOME, mapped.MATOUTCOME);
        Assert.Equal(entry.GPSOUTCOME, mapped.GPSOUTCOME);
        Assert.Equal(entry.WRITTAOUTCOME, mapped.WRITTAOUTCOME);
        Assert.Equal(entry.SCITAOUTCOME, mapped.SCITAOUTCOME);
        Assert.Equal(entry.MATTAOUTCOME, mapped.MATTAOUTCOME);
        Assert.Equal(entry.READTAOUTCOME, mapped.READTAOUTCOME);

        // Scores
        Assert.Equal(entry.READSCORE, mapped.READSCORE);
        Assert.Equal(entry.READSCORE_noSpeccon, mapped.READSCORE_noSpeccon);
        Assert.Equal(entry.MATSCORE, mapped.MATSCORE);
        Assert.Equal(entry.MATSCORE_noSpeccon, mapped.MATSCORE_noSpeccon);
        Assert.Equal(entry.GPSSCORE, mapped.GPSSCORE);
        Assert.Equal(entry.GPSSCORE_noSpeccon, mapped.GPSSCORE_noSpeccon);

        // Special conditions
        Assert.Equal(entry.READSPECCON, mapped.READSPECCON);
        Assert.Equal(entry.MATSPECCON, mapped.MATSPECCON);
        Assert.Equal(entry.GPSSPECCON, mapped.GPSSPECCON);

        // Eligibility
        Assert.Equal(entry.ELIGREAD, mapped.ELIGREAD);
        Assert.Equal(entry.ELIGMAT, mapped.ELIGMAT);
        Assert.Equal(entry.ELIGGPS, mapped.ELIGGPS);
        Assert.Equal(entry.ELIGREADTA, mapped.ELIGREADTA);
        Assert.Equal(entry.ELIGMATTA, mapped.ELIGMATTA);
        Assert.Equal(entry.ELIGSCITA, mapped.ELIGSCITA);
        Assert.Equal(entry.ELIGWRITTA, mapped.ELIGWRITTA);

        // Progress measures
        Assert.Equal(entry.INREADPROG, mapped.INREADPROG);
        Assert.Equal(entry.INWRITPROG, mapped.INWRITPROG);
        Assert.Equal(entry.INMATPROG, mapped.INMATPROG);

        // Predictions
        Assert.Equal(entry.KS2READPRED_EM, mapped.KS2READPRED_EM);
        Assert.Equal(entry.KS2WRITPRED_EM, mapped.KS2WRITPRED_EM);
        Assert.Equal(entry.KS2MATPRED_EM, mapped.KS2MATPRED_EM);

        // Adjusted progress
        Assert.Equal(entry.READPROGSCORE_EM_ADJUSTED, mapped.READPROGSCORE_EM_ADJUSTED);
        Assert.Equal(entry.WRITPROGSCORE_EM_ADJUSTED, mapped.WRITPROGSCORE_EM_ADJUSTED);
        Assert.Equal(entry.MATPROGSCORE_EM_ADJUSTED, mapped.MATPROGSCORE_EM_ADJUSTED);

        // Version
        Assert.Equal(entry.VERSION, mapped.VERSION);
    }

    [Fact]
    public void Map_HandlesNullEntriesInsideList()
    {
        NationalPupilToKs2OutputRecordMapper mapper =
            new NationalPupilToKs2OutputRecordMapper();

        NationalPupil pupil = NationalPupilTestDoubles.Create(includeKeyStages: true);

        pupil.KeyStage2!.Insert(0, null!);

        List<KS2OutputRecord> result = mapper.Map(pupil).ToList();

        Assert.Equal(pupil.KeyStage2.Count, result.Count);
        Assert.Null(result[0].PupilMatchingRef);
    }

    [Fact]
    public void Map_IgnoresUnmappedFieldsInKeyStage2Entry()
    {
        // These exist on the entry but not on the output
        Assert.Null(typeof(KS2OutputRecord).GetProperty("SomeLegacyField"));
        Assert.Null(typeof(KS2OutputRecord).GetProperty("DISC_PSENG"));
        Assert.Null(typeof(KS2OutputRecord).GetProperty("APS"));
    }
}
