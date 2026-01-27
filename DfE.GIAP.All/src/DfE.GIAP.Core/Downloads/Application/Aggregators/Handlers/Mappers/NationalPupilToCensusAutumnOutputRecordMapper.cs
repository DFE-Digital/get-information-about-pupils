using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.DownloadOutputs;
using DfE.GIAP.Core.Downloads.Application.Models.Entries;

namespace DfE.GIAP.Core.Downloads.Application.Aggregators.Handlers.Mappers;

public class NationalPupilToCensusAutumnOutputRecordMapper : IMapper<NationalPupil, CensusAutumnOutput>
{
    public CensusAutumnOutput Map(NationalPupil input)
    {
        ArgumentNullException.ThrowIfNull(input);

        CensusAutumnEntry? censusAutumnEntry = input.CensusAutumn?.FirstOrDefault();
        return new CensusAutumnOutput
        {
            PupilMatchingRef = input.PupilMatchingRef,
            UPN = input.Upn,
            Surname = input.Surname,
            Forename = input.Forename,
            MiddleNames = input.MiddleName,
            Sex = input.Sex,
            DOB = input.DOB.ToShortDateString(),
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
            EYPPR = null,
            EYPPBF = null,
            ExtendedHours = censusAutumnEntry?.ExtendedHours,
            ExpandedHours = censusAutumnEntry?.ExpandedHours,
            DAFIndicator = censusAutumnEntry?.DisabilityAccessFundIndicator ?? 0,
            Funding_Basis_ECO = null,
            Funding_Basis_HSD = null,
            Funding_Basis_LAA = null,
            TLevelQualHrs = censusAutumnEntry?.TLevelQualHrs,
            TLevelNonqualHrs = censusAutumnEntry?.TLevelNonqualHrs
        };
    }
}
