using System.Globalization;
using DfE.GIAP.Core.Common.Application.Helpers;
using DfE.GIAP.Core.Downloads.Application.DataDownloads.DownloadOutputs;
using DfE.GIAP.Core.Downloads.Application.Models;

namespace DfE.GIAP.Core.Downloads.Application.DataDownloads.Aggregators.Handlers.Mappers;

public class NationalPupilToCensusSpringOutputRecordMapper : IMapper<NationalPupil, IEnumerable<CensusSpringOutputRecord>>
{
    public IEnumerable<CensusSpringOutputRecord> Map(NationalPupil input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (input.CensusSpring is null || !input.CensusSpring.Any())
            return Enumerable.Empty<CensusSpringOutputRecord>();

        return input.CensusSpring.Select(censusSpringEntry => new CensusSpringOutputRecord
        {
            PupilMatchingRef = censusSpringEntry?.PupilMatchingRef,
            UPN = censusSpringEntry?.UniquePupilNumber,
            Surname = censusSpringEntry?.Surname,
            Forename = censusSpringEntry?.Forename,
            MiddleNames = censusSpringEntry?.MiddleNames,
            DOB = censusSpringEntry?.DOB?.ToString(DateFormatting.StandardDateFormat, CultureInfo.InvariantCulture),
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
            Ethnicity = censusSpringEntry?.Ethnicity,
            FSMeligible = censusSpringEntry?.FreeSchoolMealEligible,
            FSM_protected = censusSpringEntry?.FreeSchoolMealProtected,
            EVERFSM_6 = censusSpringEntry?.EVERFSM_6,
            EVERFSM_6_P = censusSpringEntry?.EVERFSM_6_P,
            Language = censusSpringEntry?.Language,
            HoursAtSetting = censusSpringEntry?.HoursAtSetting,
            FundedHours = censusSpringEntry?.FundedHours,
            EnrolStatus = censusSpringEntry?.EnrolStatus,
            EntryDate = censusSpringEntry?.EntryDate?.ToString(DateFormatting.StandardDateFormat, CultureInfo.InvariantCulture),
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
