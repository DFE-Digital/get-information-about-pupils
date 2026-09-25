using System.Globalization;
using DfE.GIAP.Core.Common.Application.Helpers;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilDatasets.DataDownloads.DownloadOutputs;

namespace DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilDatasets.DataDownloads.Aggregators.Handlers.Mappers;

public class NationalPupilToEyfspOutputRecordMapper : IMapper<NationalPupil, IEnumerable<EYFSPOutputRecord>>
{
    public IEnumerable<EYFSPOutputRecord> Map(NationalPupil input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (input.EarlyYearsFoundationStageProfile is null || !input.EarlyYearsFoundationStageProfile.Any())
            return Enumerable.Empty<EYFSPOutputRecord>();

        return input.EarlyYearsFoundationStageProfile.Select(eyfspEntry => new EYFSPOutputRecord
        {
            FSP_PupilMatchingRef = eyfspEntry?.PUPILMATCHINGREF,
            FSP_ACADYR = eyfspEntry?.ACADYR,
            FSP_UPN = eyfspEntry?.UPN,
            FSP_SURNAME = eyfspEntry?.SURNAME,
            FSP_FORENAME = eyfspEntry?.FORENAME,
            FSP_DOB = eyfspEntry?.DOB?.ToString(DateFormatting.StandardDateFormat, CultureInfo.InvariantCulture),
            FSP_SEX = eyfspEntry?.SEX,
            FSP_MTH_ENTRY = eyfspEntry?.MTH_ENTRY,
            FSP_LA = eyfspEntry?.LA,
            FSP_LA_9Code = eyfspEntry?.LA_9CODE,
            FSP_ESTAB = eyfspEntry?.ESTAB,
            FSP_LAESTAB = eyfspEntry?.LAESTAB,
            FSP_URN = eyfspEntry?.URN,
            FSP_NFTYPE = eyfspEntry?.NFTYPE,
            FSP_NPDPUB = eyfspEntry?.NPDPUB,
            FSP_LLSOA11 = eyfspEntry?.LLSOA11,
            FSP_IDACISCORE = eyfspEntry?.IDACISCORE,
            FSP_IDACIRANK = eyfspEntry?.IDACIRANK,
            FSP_COM_G01 = eyfspEntry?.COM_G01,
            FSP_COM_G02 = eyfspEntry?.COM_G02,
            FSP_COM_G03 = eyfspEntry?.COM_G03,
            FSP_PHY_G04 = eyfspEntry?.PHY_G04,
            FSP_PHY_G05 = eyfspEntry?.PHY_G05,
            FSP_PSE_G06 = eyfspEntry?.PSE_G06,
            FSP_PSE_G07 = eyfspEntry?.PSE_G07,
            FSP_PSE_G08 = eyfspEntry?.PSE_G08,
            FSP_LIT_G09 = eyfspEntry?.LIT_G09,
            FSP_LIT_G10 = eyfspEntry?.LIT_G10,
            FSP_MAT_G11 = eyfspEntry?.MAT_G11,
            FSP_MAT_G12 = eyfspEntry?.MAT_G12,
            FSP_UTW_G13 = eyfspEntry?.UTW_G13,
            FSP_UTW_G14 = eyfspEntry?.UTW_G14,
            FSP_UTW_G15 = eyfspEntry?.UTW_G15,
            FSP_EXP_G16 = eyfspEntry?.EXP_G16,
            FSP_EXP_G17 = eyfspEntry?.EXP_G17,
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
        });
    }
}
