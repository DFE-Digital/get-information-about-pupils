using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.DownloadOutputs;

namespace DfE.GIAP.Core.Downloads.Application.Pupils.Aggregators.Handlers.Mappers;

public class NationalPupilToCensusSummerOutputRecordMapper : IMapper<NationalPupil, CensusSummerOutput>
{
    public CensusSummerOutput Map(NationalPupil input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return new CensusSummerOutput
        {

        };
    }
}
