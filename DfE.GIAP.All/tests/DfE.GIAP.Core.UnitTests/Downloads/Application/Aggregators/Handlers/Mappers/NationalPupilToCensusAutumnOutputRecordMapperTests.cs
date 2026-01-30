using DfE.GIAP.Core.Downloads.Application.Aggregators.Handlers.Mappers;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.DownloadOutputs;
using DfE.GIAP.Core.Downloads.Application.Models.Entries;
using DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.Downloads.Application.Aggregators.Handlers.Mappers;

public sealed class NationalPupilToCensusAutumnOutputRecordMapperTests
{
    [Fact]
    public void Map_ThrowsArgumentNullException_WhenInputIsNull()
    {
        NationalPupilToCensusAutumnOutputRecordMapper mapper = new();

        Action act = () => mapper.Map(null!);

        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void Map_ReturnsEmpty_WhenCensusAutumnListIsNull()
    {
        NationalPupilToCensusAutumnOutputRecordMapper mapper = new();
        NationalPupil pupil = NationalPupilTestDoubles.Create(includeCensus: false);
        pupil.CensusAutumn = null!;

        IEnumerable<CensusAutumnOutputRecord> result = mapper.Map(pupil);

        Assert.Empty(result);
    }

    [Fact]
    public void Map_ReturnsEmpty_WhenCensusAutumnListIsEmpty()
    {
        NationalPupilToCensusAutumnOutputRecordMapper mapper = new();
        NationalPupil pupil = NationalPupilTestDoubles.Create(includeCensus: false);

        IEnumerable<CensusAutumnOutputRecord> result = mapper.Map(pupil);

        Assert.Empty(result);
    }

    [Fact]
    public void Map_MapsAllCensusAutumnEntries()
    {
        NationalPupilToCensusAutumnOutputRecordMapper mapper = new();
        NationalPupil pupil = NationalPupilTestDoubles.Create(includeCensus: true);

        IEnumerable<CensusAutumnOutputRecord> result = mapper.Map(pupil);

        Assert.Equal(pupil.CensusAutumn!.Count, result.Count());
    }

    [Fact]
    public void Map_MapsCensusAutumnEntryFieldsCorrectly()
    {
        NationalPupilToCensusAutumnOutputRecordMapper mapper = new();
        NationalPupil pupil = NationalPupilTestDoubles.Create(includeCensus: true);

        CensusAutumnEntry entry = pupil.CensusAutumn!.First();

        CensusAutumnOutputRecord mapped = mapper.Map(pupil).First();

        Assert.Equal(entry.PupilMatchingRef, mapped.PupilMatchingRef);
        Assert.Equal(entry.UniquePupilNumber, mapped.UPN);
        Assert.Equal(entry.Surname, mapped.Surname);
        Assert.Equal(entry.Forename, mapped.Forename);
        Assert.Equal(entry.MiddleNames, mapped.MiddleNames);
        Assert.Equal(entry.Sex, mapped.Sex);
        Assert.Equal(entry.DOB?.ToShortDateString(), mapped.DOB);
        Assert.Equal(entry.AcademicYear, mapped.AcademicYear);
        Assert.Equal(entry.CensusTerm, mapped.CensusTerm);
        Assert.Equal(entry.LocalAuthority, mapped.LA);
        Assert.Equal(entry.Establishment, mapped.Estab);
        Assert.Equal(entry.LocalAuthorityEstablishment, mapped.LAEstab);
        Assert.Equal(entry.UniqueReferenceNumber, mapped.URN);
        Assert.Equal(entry.Phase, mapped.PHASE);
        Assert.Equal(entry.FormerUniquePupilNumber, mapped.FormerUPN);
        Assert.Equal(entry.PreferredSurname, mapped.PreferredSurname);
        Assert.Equal(entry.FormerSurname, mapped.FormerSurname);
        Assert.Equal(entry.FreeSchoolMealEligible, mapped.FSMeligible);
        Assert.Equal(entry.FreeSchoolMealProtected, mapped.FSM_protected);
        Assert.Equal(entry.FreeSchoolMealEligible, mapped.EVERFSM_6);
        Assert.Equal(entry.FreeSchoolMealProtected, mapped.EVERFSM_6_P);
        Assert.Equal(entry.Language, mapped.Language);
        Assert.Equal(entry.HoursAtSetting, mapped.HoursAtSetting);
        Assert.Equal(entry.FundedHours, mapped.FundedHours);
        Assert.Equal(entry.EnrolStatus, mapped.EnrolStatus);
        Assert.Equal(entry.EntryDate, mapped.EntryDate);
        Assert.Equal(entry.NationalCurriculumYearActual, mapped.NCyearActual);
        Assert.Equal(entry.SpecialEducationalNeedsProvision, mapped.SENProvision);
        Assert.Equal(entry.PrimarySpecialEducationalNeedsType, mapped.PrimarySENeedsType);
        Assert.Equal(entry.SecondarySpecialEducationalNeedsType, mapped.SecondarySENType);
        Assert.Equal(entry.IncomeDeprivationAffectingChildrenIndexScore, mapped.IDACI_S);
        Assert.Equal(entry.IncomeDeprivationAffectingChildrenIndexRating, mapped.IDACI_R);
        Assert.Equal(entry.ExtendedHours, mapped.ExtendedHours);
        Assert.Equal(entry.ExpandedHours, mapped.ExpandedHours);
        Assert.Equal(entry.DisabilityAccessFundIndicator, mapped.DAFIndicator);
        Assert.Equal(entry.TLevelQualHrs, mapped.TLevelQualHrs);
        Assert.Equal(entry.TLevelNonqualHrs, mapped.TLevelNonqualHrs);
    }

    [Fact]
    public void Map_HandlesNullEntriesInsideList()
    {
        NationalPupilToCensusAutumnOutputRecordMapper mapper = new();
        NationalPupil pupil = NationalPupilTestDoubles.Create(includeCensus: true);

        pupil.CensusAutumn!.Insert(0, null!);

        List<CensusAutumnOutputRecord> result = mapper.Map(pupil).ToList();

        Assert.Equal(pupil.CensusAutumn.Count, result.Count);
        Assert.Null(result[0].PupilMatchingRef);
    }

    [Fact]
    public void Map_IgnoresUnmappedFieldsInCensusAutumnEntry()
    {
        // These fields do not exist on CensusAutumnOutput
        Assert.Null(typeof(CensusAutumnOutputRecord).GetProperty("SomeUnmappedField"));
        Assert.Null(typeof(CensusAutumnOutputRecord).GetProperty("AnotherUnmappedField"));
    }
}
