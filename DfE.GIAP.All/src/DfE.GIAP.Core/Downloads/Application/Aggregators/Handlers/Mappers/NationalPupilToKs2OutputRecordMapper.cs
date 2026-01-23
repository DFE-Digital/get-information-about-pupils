using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.DownloadOutputs;

namespace DfE.GIAP.Core.Downloads.Application.Aggregators.Handlers.Mappers;

public class NationalPupilToKs2OutputRecordMapper : IMapper<NationalPupil, KS2Output>
{
    public KS2Output Map(NationalPupil input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return new KS2Output
        {

        };
    }
}
