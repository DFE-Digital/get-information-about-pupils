using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.DownloadOutputs;

namespace DfE.GIAP.Core.Downloads.Application.Aggregators.Handlers.Mappers;

public class NationalPupilToKs4OutputRecordMapper : IMapper<NationalPupil, KS4Output>
{
    public KS4Output Map(NationalPupil input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return new KS4Output
        {

        };
    }
}
