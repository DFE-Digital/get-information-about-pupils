using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.DownloadOutputs;
using DfE.GIAP.Core.Downloads.Application.Models.Entries;

namespace DfE.GIAP.Core.Downloads.Application.Aggregators.Handlers.Mappers;

public class NationalPupilToEyfspOutputRecordMapper : IMapper<NationalPupil, EYFSPOutput>
{
    public EYFSPOutput Map(NationalPupil input)
    {
        ArgumentNullException.ThrowIfNull(input);

        EarlyYearsFoundationStageProfileEntry? eyfspEntry = input.EarlyYearsFoundationStageProfile?.FirstOrDefault();
        return new EYFSPOutput
        {
            FSP_PupilMatchingRef = eyfspEntry?.PUPILMATCHINGREF,
            FSP_ACADYR = eyfspEntry?.ACADYR,
            FSP_UPN = eyfspEntry?.UPN,
            FSP_SURNAME = eyfspEntry?.SURNAME,
            FSP_FORENAME = eyfspEntry?.FORENAME,
            FSP_DOB = eyfspEntry?.DOB,
            FSP_SEX = eyfspEntry?.SEX,
            FSP_MTH_ENTRY = eyfspEntry?.MTH_ENTRY,
            FSP_LA = eyfspEntry?.LA,
            FSP_LA_9CODE = eyfspEntry?.LA_9CODE,
            FSP_ESTAB = eyfspEntry?.ESTAB,
            FSP_LAESTAB = eyfspEntry?.LAESTAB,
            FSP_URN = eyfspEntry?.URN,
            FSP_NFTYPE = eyfspEntry?.NFTYPE,
            FSP_NPDPUB = eyfspEntry?.NPDPUB,
            FSP_LLSOA11 = eyfspEntry?.LLSOA11,
            FSP_IDACISCORE = eyfspEntry?.IDACISCORE,
            FSP_IDACIRANK = eyfspEntry?.IDACIRANK,
            FSP_COM_E01 = eyfspEntry?.COM_E01,
            FSP_COM_E02 = eyfspEntry?.COM_E02,
            FSP_PSE_E03 = eyfspEntry?.PSE_E03,
            FSP_PSE_E04 = eyfspEntry?.PSE_E04,
            FSP_PSE_E05 = eyfspEntry?.PSE_E05,
            FSP_PHY_E06 = eyfspEntry?.PHY_E06,
            FSP_PHY_E07 = eyfspEntry?.PHY_E07,
            FSP_LIT_E08 = eyfspEntry?.LIT_E08,
            FSP_LIT_E09 = eyfspEntry?.LIT_E09,
            FSP_LIT_E10 = eyfspEntry?.LIT_E10,
            FSP_MAT_E11 = eyfspEntry?.MAT_E11,
            FSP_MAT_E12 = eyfspEntry?.MAT_E12,
            FSP_UTW_E13 = eyfspEntry?.UTW_E13,
            FSP_UTW_E14 = eyfspEntry?.UTW_E14,
            FSP_UTW_E15 = eyfspEntry?.UTW_E15,
            FSP_EXP_E16 = eyfspEntry?.EXP_E16,
            FSP_EXP_E17 = eyfspEntry?.EXP_E17,
            FSP_AGE = eyfspEntry?.AGE,
            FSP_GLD = eyfspEntry?.GLD,
            FSP_VERSION = eyfspEntry?.VERSION
        };
    }
}
