using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.DownloadOutputs;

namespace DfE.GIAP.Core.Downloads.Application.Aggregators.Handlers.Mappers;

public class NationalPupilToCensusSummerOutputRecordMapper : IMapper<NationalPupil, IEnumerable<CensusSummerOutputRecord>>
{
    public IEnumerable<CensusSummerOutputRecord> Map(NationalPupil input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (input.CensusSummer is null || !input.CensusSummer.Any())
            return Enumerable.Empty<CensusSummerOutputRecord>();

        return input.CensusSummer.Select(censusSummerEntry => new CensusSummerOutputRecord
        {
            PupilMatchingRef = censusSummerEntry?.PupilMatchingRef,
            AcademicYear = censusSummerEntry?.AcademicYear,
            CensusTerm = censusSummerEntry?.CensusTerm,
            LA = censusSummerEntry?.LocalAuthority,
            Estab = censusSummerEntry?.Establishment,
            LAEstab = censusSummerEntry?.LocalAuthorityEstablishment,
            URN = censusSummerEntry?.UniqueReferenceNumber,
            PHASE = censusSummerEntry?.Phase,
            UPN = censusSummerEntry?.UniquePupilNumber,
            FormerUPN = censusSummerEntry?.FormerUniquePupilNumber,
            Surname = censusSummerEntry?.Surname,
            Forename = censusSummerEntry?.Forename,
            MiddleNames = censusSummerEntry?.MiddleNames,
            PreferredSurname = censusSummerEntry?.PreferredSurname,
            FormerSurname = censusSummerEntry?.FormerSurname,
            Sex = censusSummerEntry?.Sex,
            DOB = censusSummerEntry?.DOB?.ToShortDateString(),
            FSMeligible = censusSummerEntry?.FreeSchoolMealEligible,
            FSM_protected = censusSummerEntry?.FreeSchoolMealProtected,
            EVERFSM_6 = censusSummerEntry?.EVERFSM_6,
            EVERFSM_6_P = censusSummerEntry?.EVERFSM_6_P,
            Language = censusSummerEntry?.Language,
            HoursAtSetting = censusSummerEntry?.HoursAtSetting,
            FundedHours = censusSummerEntry?.FundedHours,
            EnrolStatus = censusSummerEntry?.EnrolStatus,
            EntryDate = censusSummerEntry?.EntryDate,
            NCyearActual = censusSummerEntry?.NationalCurriculumYearActual,
            SENProvision = censusSummerEntry?.SpecialEducationalNeedsProvision,
            PrimarySENeedsType = censusSummerEntry?.PrimarySpecialEducationalNeedsType,
            SecondarySENType = censusSummerEntry?.SecondarySpecialEducationalNeedsType,
            IDACI_S = censusSummerEntry?.IncomeDeprivationAffectingChildrenIndexScore,
            IDACI_R = censusSummerEntry?.IncomeDeprivationAffectingChildrenIndexRating,
            ExtendedHours = censusSummerEntry?.ExtendedHours,
            ExpandedHours = censusSummerEntry?.ExpandedHours,
            DAFIndicator = censusSummerEntry?.DisabilityAccessFundIndicator,
        });
    }
}
