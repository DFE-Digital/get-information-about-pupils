using System.Globalization;
using DfE.GIAP.Core.Common.Application.Helpers;
using DfE.GIAP.Core.Downloads.Application.DataDownloads.Aggregators.Handlers.Mappers;
using DfE.GIAP.Core.Downloads.Application.DataDownloads.DownloadOutputs;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.Entries;
using DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.Downloads.Application.Aggregators.Handlers.Mappers;

public sealed class NationalPupilToCensusSummerOutputRecordMapperTests
{
    [Fact]
    public void Map_ThrowsArgumentNullException_WhenInputIsNull()
    {
        NationalPupilToCensusSummerOutputRecordMapper mapper =
            new NationalPupilToCensusSummerOutputRecordMapper();

        Action act = () => mapper.Map(null!);

        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void Map_ReturnsEmpty_WhenCensusSummerListIsNull()
    {
        NationalPupilToCensusSummerOutputRecordMapper mapper =
            new NationalPupilToCensusSummerOutputRecordMapper();

        NationalPupil pupil = NationalPupilTestDoubles.Create(includeCensus: false);
        pupil.CensusSummer = null!;

        IEnumerable<CensusSummerOutputRecord> result = mapper.Map(pupil);

        Assert.Empty(result);
    }

    [Fact]
    public void Map_ReturnsEmpty_WhenCensusSummerListIsEmpty()
    {
        NationalPupilToCensusSummerOutputRecordMapper mapper =
            new NationalPupilToCensusSummerOutputRecordMapper();

        NationalPupil pupil = NationalPupilTestDoubles.Create(includeCensus: false);

        IEnumerable<CensusSummerOutputRecord> result = mapper.Map(pupil);

        Assert.Empty(result);
    }

    [Fact]
    public void Map_MapsAllCensusSummerEntries()
    {
        NationalPupilToCensusSummerOutputRecordMapper mapper =
            new NationalPupilToCensusSummerOutputRecordMapper();

        NationalPupil pupil = NationalPupilTestDoubles.Create(includeCensus: true);

        IEnumerable<CensusSummerOutputRecord> result = mapper.Map(pupil);

        Assert.Equal(pupil.CensusSummer!.Count, result.Count());
    }

    [Fact]
    public void Map_MapsCensusSummerEntryFieldsCorrectly()
    {
        NationalPupilToCensusSummerOutputRecordMapper mapper =
            new NationalPupilToCensusSummerOutputRecordMapper();

        NationalPupil pupil = NationalPupilTestDoubles.Create(includeCensus: true);

        CensusSummerEntry entry = pupil.CensusSummer!.First();

        CensusSummerOutputRecord mapped = mapper.Map(pupil).First();

        Assert.Equal(entry.PupilMatchingRef, mapped.PupilMatchingRef);
        Assert.Equal(entry.AcademicYear, mapped.AcademicYear);
        Assert.Equal(entry.CensusTerm, mapped.CensusTerm);
        Assert.Equal(entry.LocalAuthority, mapped.LA);
        Assert.Equal(entry.Establishment, mapped.Estab);
        Assert.Equal(entry.LocalAuthorityEstablishment, mapped.LAEstab);
        Assert.Equal(entry.UniqueReferenceNumber, mapped.URN);
        Assert.Equal(entry.Phase, mapped.PHASE);
        Assert.Equal(entry.UniquePupilNumber, mapped.UPN);
        Assert.Equal(entry.FormerUniquePupilNumber, mapped.FormerUPN);
        Assert.Equal(entry.Surname, mapped.Surname);
        Assert.Equal(entry.Forename, mapped.Forename);
        Assert.Equal(entry.MiddleNames, mapped.MiddleNames);
        Assert.Equal(entry.PreferredSurname, mapped.PreferredSurname);
        Assert.Equal(entry.FormerSurname, mapped.FormerSurname);
        Assert.Equal(entry.Sex, mapped.Sex);
        Assert.Equal(entry.DOB?.ToString(DateFormatting.StandardDateFormat, CultureInfo.InvariantCulture), mapped.DOB);
        Assert.Equal(entry.FreeSchoolMealEligible, mapped.FSMeligible);
        Assert.Equal(entry.FreeSchoolMealProtected, mapped.FSM_protected);
        Assert.Equal(entry.EVERFSM_6, mapped.EVERFSM_6);
        Assert.Equal(entry.EVERFSM_6_P, mapped.EVERFSM_6_P);
        Assert.Equal(entry.Language, mapped.Language);
        Assert.Equal(entry.HoursAtSetting, mapped.HoursAtSetting);
        Assert.Equal(entry.FundedHours, mapped.FundedHours);
        Assert.Equal(entry.EnrolStatus, mapped.EnrolStatus);
        Assert.Equal(entry.EntryDate?.ToString(DateFormatting.StandardDateFormat, CultureInfo.InvariantCulture), mapped.EntryDate);
        Assert.Equal(entry.NationalCurriculumYearActual, mapped.NCyearActual);
        Assert.Equal(entry.SpecialEducationalNeedsProvision, mapped.SENProvision);
        Assert.Equal(entry.PrimarySpecialEducationalNeedsType, mapped.PrimarySENeedsType);
        Assert.Equal(entry.SecondarySpecialEducationalNeedsType, mapped.SecondarySENType);
        Assert.Equal(entry.IncomeDeprivationAffectingChildrenIndexScore, mapped.IDACI_S);
        Assert.Equal(entry.IncomeDeprivationAffectingChildrenIndexRating, mapped.IDACI_R);
        Assert.Equal(entry.ExtendedHours, mapped.ExtendedHours);
        Assert.Equal(entry.ExpandedHours, mapped.ExpandedHours);
        Assert.Equal(entry.DisabilityAccessFundIndicator, mapped.DAFIndicator);
    }

    [Fact]
    public void Map_HandlesNullEntriesInsideList()
    {
        NationalPupilToCensusSummerOutputRecordMapper mapper =
            new NationalPupilToCensusSummerOutputRecordMapper();

        NationalPupil pupil = NationalPupilTestDoubles.Create(includeCensus: true);

        pupil.CensusSummer!.Insert(0, null!);

        List<CensusSummerOutputRecord> result = mapper.Map(pupil).ToList();

        Assert.Equal(pupil.CensusSummer.Count, result.Count);
        Assert.Null(result[0].PupilMatchingRef);
    }

    [Fact]
    public void Map_IgnoresUnmappedFieldsInCensusSummerEntry()
    {
        // These exist on the entry but not on the output
        Assert.Null(typeof(CensusSummerOutputRecord).GetProperty("ServiceChild"));
        Assert.Null(typeof(CensusSummerOutputRecord).GetProperty("Gender"));
    }
}
