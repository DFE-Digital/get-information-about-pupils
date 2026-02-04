using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DfE.GIAP.Core.Common.Application.Helpers;
using DfE.GIAP.Core.Downloads.Application.Aggregators.Handlers.Mappers;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.DownloadOutputs;
using DfE.GIAP.Core.Downloads.Application.Models.Entries;
using DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;
using Xunit;

namespace DfE.GIAP.Core.UnitTests.Downloads.Application.Aggregators.Handlers.Mappers;

public sealed class NationalPupilToPhonicsOutputRecordMapperTests
{
    [Fact]
    public void Map_ThrowsArgumentNullException_WhenInputIsNull()
    {
        NationalPupilToPhonicsOutputRecordMapper mapper =
            new NationalPupilToPhonicsOutputRecordMapper();

        Action act = () => mapper.Map(null!);

        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void Map_ReturnsEmpty_WhenPhonicsListIsNull()
    {
        NationalPupilToPhonicsOutputRecordMapper mapper =
            new NationalPupilToPhonicsOutputRecordMapper();

        NationalPupil pupil = NationalPupilTestDoubles.Create(includePhonics: false);
        pupil.Phonics = null!;

        IEnumerable<PhonicsOutputRecord> result = mapper.Map(pupil);

        Assert.Empty(result);
    }

    [Fact]
    public void Map_ReturnsEmpty_WhenPhonicsListIsEmpty()
    {
        NationalPupilToPhonicsOutputRecordMapper mapper =
            new NationalPupilToPhonicsOutputRecordMapper();

        NationalPupil pupil = NationalPupilTestDoubles.Create(includePhonics: false);

        IEnumerable<PhonicsOutputRecord> result = mapper.Map(pupil);

        Assert.Empty(result);
    }

    [Fact]
    public void Map_MapsAllPhonicsEntries()
    {
        NationalPupilToPhonicsOutputRecordMapper mapper =
            new NationalPupilToPhonicsOutputRecordMapper();

        NationalPupil pupil = NationalPupilTestDoubles.Create(includePhonics: true);

        IEnumerable<PhonicsOutputRecord> result = mapper.Map(pupil);

        Assert.Equal(pupil.Phonics!.Count, result.Count());
    }

    [Fact]
    public void Map_MapsPhonicsEntryFieldsCorrectly()
    {
        NationalPupilToPhonicsOutputRecordMapper mapper =
            new NationalPupilToPhonicsOutputRecordMapper();

        NationalPupil pupil = NationalPupilTestDoubles.Create(includePhonics: true);

        PhonicsEntry entry = pupil.Phonics!.First();

        PhonicsOutputRecord mapped = mapper.Map(pupil).First();

        Assert.Equal(entry.AcademicYear, mapped.Phonics_ACADYR);
        Assert.Equal(entry.PupilMatchingReference, mapped.Phonics_PUPILMATCHINGREF);
        Assert.Equal(entry.Id, mapped.Phonics_ID);
        Assert.Equal(entry.UniquePupilNumber, mapped.Phonics_UPN);
        Assert.Equal(entry.SurName, mapped.Phonics_SURNAME);
        Assert.Equal(entry.ForeNames, mapped.Phonics_FORENAMES);
        Assert.Equal(entry.DOB?.ToString(DateFormatting.StandardDateFormat, CultureInfo.InvariantCulture), mapped.Phonics_DOB);
        Assert.Equal(entry.SEX, mapped.Phonics_SEX);
        Assert.Equal(entry.LocalAuthority, mapped.Phonics_LA);
        Assert.Equal(entry.Establishment, mapped.Phonics_ESTAB);
        Assert.Equal(entry.UniqueReferenceNumber, mapped.Phonics_URN);
        Assert.Equal(entry.NationalCurriculumYearActual, mapped.Phonics_NCYEARACTUAL);
        Assert.Equal(entry.TypeOfEstablishmentCode, mapped.Phonics_TOE_CODE);
        Assert.Equal(entry.Phonics_Mark, mapped.Phonics_PHONICS_MARK);
        Assert.Equal(entry.Phonics_Outcome, mapped.Phonics_PHONICS_OUTCOME);
        Assert.Equal(entry.Version, mapped.Phonics_VERSION);
    }

    [Fact]
    public void Map_HandlesNullEntriesInsideList()
    {
        NationalPupilToPhonicsOutputRecordMapper mapper =
            new NationalPupilToPhonicsOutputRecordMapper();

        NationalPupil pupil = NationalPupilTestDoubles.Create(includePhonics: true);

        pupil.Phonics!.Insert(0, null!);

        List<PhonicsOutputRecord> result = mapper.Map(pupil).ToList();

        Assert.Equal(pupil.Phonics.Count, result.Count);
        Assert.Null(result[0].Phonics_PUPILMATCHINGREF);
    }

    [Fact]
    public void Map_IgnoresUnmappedFieldsInPhonicsEntry()
    {
        // These exist on the entry but not on the output
        Assert.Null(typeof(PhonicsOutputRecord).GetProperty("Gender"));
        Assert.Null(typeof(PhonicsOutputRecord).GetProperty("Phonics_Mark_Aut21"));
        Assert.Null(typeof(PhonicsOutputRecord).GetProperty("Phonics_Outcome_Aut21"));
    }
}
