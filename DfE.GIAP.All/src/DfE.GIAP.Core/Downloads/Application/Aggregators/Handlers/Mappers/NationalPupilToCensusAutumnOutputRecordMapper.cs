using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.DownloadOutputs;

namespace DfE.GIAP.Core.Downloads.Application.Aggregators.Handlers.Mappers;

public class NationalPupilToCensusAutumnOutputRecordMapper : IMapper<NationalPupil, IEnumerable<CensusAutumnOutput>>
{
    public IEnumerable<CensusAutumnOutput> Map(NationalPupil input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (input.CensusAutumn is null || !input.CensusAutumn.Any())
            return Enumerable.Empty<CensusAutumnOutput>();

        return input.CensusAutumn.Select(censusAutumnEntry => new CensusAutumnOutput
        {
            PupilMatchingRef = censusAutumnEntry?.PupilMatchingRef,
            UPN = censusAutumnEntry?.UniquePupilNumber,
            Surname = censusAutumnEntry?.Surname,
            Forename = censusAutumnEntry?.Forename,
            MiddleNames = censusAutumnEntry?.MiddleNames,
            Sex = censusAutumnEntry?.Sex,
            DOB = censusAutumnEntry?.DOB?.ToShortDateString(),
            AcademicYear = censusAutumnEntry?.AcademicYear,
            CensusTerm = censusAutumnEntry?.CensusTerm,
            LA = censusAutumnEntry?.LocalAuthority,
            Estab = censusAutumnEntry?.Establishment,
            LAEstab = censusAutumnEntry?.LocalAuthorityEstablishment,
            URN = censusAutumnEntry?.UniqueReferenceNumber,
            PHASE = censusAutumnEntry?.Phase,
            FormerUPN = censusAutumnEntry?.FormerUniquePupilNumber,
            PreferredSurname = censusAutumnEntry?.PreferredSurname,
            FormerSurname = censusAutumnEntry?.FormerSurname,
            FSMeligible = censusAutumnEntry?.FreeSchoolMealEligible,
            FSM_protected = censusAutumnEntry?.FreeSchoolMealProtected,
            EVERFSM_6 = censusAutumnEntry?.FreeSchoolMealEligible,
            EVERFSM_6_P = censusAutumnEntry?.FreeSchoolMealProtected,
            Language = censusAutumnEntry?.Language,
            HoursAtSetting = censusAutumnEntry?.HoursAtSetting,
            FundedHours = censusAutumnEntry?.FundedHours,
            EnrolStatus = censusAutumnEntry?.EnrolStatus,
            EntryDate = censusAutumnEntry?.EntryDate,
            NCyearActual = censusAutumnEntry?.NationalCurriculumYearActual,
            SENProvision = censusAutumnEntry?.SpecialEducationalNeedsProvision,
            PrimarySENeedsType = censusAutumnEntry?.PrimarySpecialEducationalNeedsType,
            SecondarySENType = censusAutumnEntry?.SecondarySpecialEducationalNeedsType,
            IDACI_S = censusAutumnEntry?.IncomeDeprivationAffectingChildrenIndexScore,
            IDACI_R = censusAutumnEntry?.IncomeDeprivationAffectingChildrenIndexRating,
            ExtendedHours = censusAutumnEntry?.ExtendedHours,
            ExpandedHours = censusAutumnEntry?.ExpandedHours,
            DAFIndicator = censusAutumnEntry?.DisabilityAccessFundIndicator ?? 0,
            TLevelQualHrs = censusAutumnEntry?.TLevelQualHrs,
            TLevelNonqualHrs = censusAutumnEntry?.TLevelNonqualHrs
        });
    }
}
