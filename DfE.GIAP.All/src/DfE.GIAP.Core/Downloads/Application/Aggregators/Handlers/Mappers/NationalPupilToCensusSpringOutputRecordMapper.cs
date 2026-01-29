using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.DownloadOutputs;

namespace DfE.GIAP.Core.Downloads.Application.Aggregators.Handlers.Mappers;

public class NationalPupilToCensusSpringOutputRecordMapper : IMapper<NationalPupil, IEnumerable<CensusSpringOutput>>
{
    public IEnumerable<CensusSpringOutput> Map(NationalPupil input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (input.CensusSpring is null || !input.CensusSpring.Any())
            return Enumerable.Empty<CensusSpringOutput>();

        return input.CensusSpring.Select(censusSpringEntry => new CensusSpringOutput
        {
            PupilMatchingRef = censusSpringEntry?.PupilMatchingRef,
            UPN = censusSpringEntry?.UniquePupilNumber,
            Surname = censusSpringEntry?.Surname,
            Forename = censusSpringEntry?.Forename,
            MiddleNames = censusSpringEntry?.MiddleNames,
            DOB = censusSpringEntry?.DOB.ToShortDateString(),
            Sex = censusSpringEntry?.Sex,
            AcademicYear = censusSpringEntry?.AcademicYear,
            CensusTerm = censusSpringEntry?.CensusTerm,
            LA = censusSpringEntry?.LocalAuthority,
            Estab = censusSpringEntry?.Establishment,
            LAEstab = censusSpringEntry?.LocalAuthorityEstablishment,
            URN = censusSpringEntry?.UniqueReferenceNumber,
            PHASE = censusSpringEntry?.Phase,
            FormerUPN = censusSpringEntry?.FormerUniquePupilNumber,
            PreferredSurname = censusSpringEntry?.PreferredSurname,
            FormerSurname = censusSpringEntry?.FormerSurname,
            FSMeligible = censusSpringEntry?.FreeSchoolMealEligible,
            FSM_protected = censusSpringEntry?.FreeSchoolMealProtected,
            EVERFSM_6 = censusSpringEntry?.EVERFSM_6,
            EVERFSM_6_P = censusSpringEntry?.EVERFSM_6_P,
            Language = censusSpringEntry?.Language,
            HoursAtSetting = censusSpringEntry?.HoursAtSetting,
            FundedHours = censusSpringEntry?.FundedHours,
            EnrolStatus = censusSpringEntry?.EnrolStatus,
            EntryDate = censusSpringEntry?.EntryDate,
            NCyearActual = censusSpringEntry?.NationalCurriculumYearActual,
            SENProvision = censusSpringEntry?.SpecialEducationalNeedsProvision,
            PrimarySENeedsType = censusSpringEntry?.PrimarySpecialEducationalNeedsType,
            SecondarySENType = censusSpringEntry?.SecondarySpecialEducationalNeedsType,
            IDACI_S = censusSpringEntry?.IncomeDeprivationAffectingChildrenIndexScore,
            IDACI_R = censusSpringEntry?.IncomeDeprivationAffectingChildrenIndexRating,
            EYPPR = censusSpringEntry?.EYPPR,
            EYPPBF = censusSpringEntry?.EYPPBF,
            ExtendedHours = censusSpringEntry?.ExtendedHours,
            ExpandedHours = censusSpringEntry?.ExpandedHours,
            DAFIndicator = censusSpringEntry?.DisabilityAccessFundIndicator,
            Funding_Basis_ECO = censusSpringEntry?.Funding_Basis_ECO,
            Funding_Basis_HSD = censusSpringEntry?.Funding_Basis_HSD,
            Funding_Basis_LAA = censusSpringEntry?.Funding_Basis_LAA,
        });
    }
}
