using System.Globalization;
using DfE.GIAP.Core.Common.Application.Helpers;
using DfE.GIAP.Core.Downloads.Application.Aggregators.Handlers.Mappers;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.DownloadOutputs;
using DfE.GIAP.Core.Downloads.Application.Models.Entries;
using DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.Downloads.Application.Aggregators.Handlers.Mappers;

public sealed class NationalPupilToCensusSpringOutputRecordMapperTests
{
    [Fact]
    public void Map_ThrowsArgumentNullException_WhenInputIsNull()
    {
        NationalPupilToCensusSpringOutputRecordMapper mapper =
            new NationalPupilToCensusSpringOutputRecordMapper();

        Action act = () => mapper.Map(null!);

        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void Map_ReturnsEmpty_WhenCensusSpringListIsNull()
    {
        NationalPupilToCensusSpringOutputRecordMapper mapper =
            new NationalPupilToCensusSpringOutputRecordMapper();

        NationalPupil pupil = NationalPupilTestDoubles.Create(includeCensus: false);
        pupil.CensusSpring = null!;

        IEnumerable<CensusSpringOutputRecord> result = mapper.Map(pupil);

        Assert.Empty(result);
    }

    [Fact]
    public void Map_ReturnsEmpty_WhenCensusSpringListIsEmpty()
    {
        NationalPupilToCensusSpringOutputRecordMapper mapper =
            new NationalPupilToCensusSpringOutputRecordMapper();

        NationalPupil pupil = NationalPupilTestDoubles.Create(includeCensus: false);

        IEnumerable<CensusSpringOutputRecord> result = mapper.Map(pupil);

        Assert.Empty(result);
    }

    [Fact]
    public void Map_MapsAllCensusSpringEntries()
    {
        NationalPupilToCensusSpringOutputRecordMapper mapper =
            new NationalPupilToCensusSpringOutputRecordMapper();

        NationalPupil pupil = NationalPupilTestDoubles.Create(includeCensus: true);

        IEnumerable<CensusSpringOutputRecord> result = mapper.Map(pupil);

        Assert.Equal(pupil.CensusSpring!.Count, result.Count());
    }

    [Fact]
    public void Map_MapsCensusSpringEntryFieldsCorrectly()
    {
        NationalPupilToCensusSpringOutputRecordMapper mapper =
            new NationalPupilToCensusSpringOutputRecordMapper();

        NationalPupil pupil = NationalPupilTestDoubles.Create(includeCensus: true);

        CensusSpringEntry entry = pupil.CensusSpring!.First();

        CensusSpringOutputRecord mapped = mapper.Map(pupil).First();

        Assert.Equal(entry.PupilMatchingRef, mapped.PupilMatchingRef);
        Assert.Equal(entry.UniquePupilNumber, mapped.UPN);
        Assert.Equal(entry.Surname, mapped.Surname);
        Assert.Equal(entry.Forename, mapped.Forename);
        Assert.Equal(entry.MiddleNames, mapped.MiddleNames);
        Assert.Equal(entry.DOB?.ToString(DateFormatting.StandardDateFormat, CultureInfo.InvariantCulture), mapped.DOB);
        Assert.Equal(entry.Sex, mapped.Sex);
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
        Assert.Equal(entry.EYPPR, mapped.EYPPR);
        Assert.Equal(entry.EYPPBF, mapped.EYPPBF);
        Assert.Equal(entry.ExtendedHours, mapped.ExtendedHours);
        Assert.Equal(entry.ExpandedHours, mapped.ExpandedHours);
        Assert.Equal(entry.DisabilityAccessFundIndicator, mapped.DAFIndicator);
        Assert.Equal(entry.Funding_Basis_ECO, mapped.Funding_Basis_ECO);
        Assert.Equal(entry.Funding_Basis_HSD, mapped.Funding_Basis_HSD);
        Assert.Equal(entry.Funding_Basis_LAA, mapped.Funding_Basis_LAA);
    }

    [Fact]
    public void Map_HandlesNullEntriesInsideList()
    {
        NationalPupilToCensusSpringOutputRecordMapper mapper =
            new NationalPupilToCensusSpringOutputRecordMapper();

        NationalPupil pupil = NationalPupilTestDoubles.Create(includeCensus: true);

        pupil.CensusSpring!.Insert(0, null!);

        List<CensusSpringOutputRecord> result = mapper.Map(pupil).ToList();

        Assert.Equal(pupil.CensusSpring.Count, result.Count);
        Assert.Null(result[0].PupilMatchingRef);
    }

    [Fact]
    public void Map_IgnoresUnmappedFieldsInCensusSpringEntry()
    {
        // These fields exist on the entry but not on the output
        Assert.Null(typeof(CensusSpringOutputRecord).GetProperty("Ethnicity"));
        Assert.Null(typeof(CensusSpringOutputRecord).GetProperty("Gender"));
    }
}
