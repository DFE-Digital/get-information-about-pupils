using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.DownloadOutputs;

namespace DfE.GIAP.Core.Downloads.Application.Pupils.Aggregators.Handlers.Mappers;

public class NationalPupilToKs1OutputRecordMapper : IMapper<NationalPupil, KS1Output>
{
    public KS1Output Map(NationalPupil input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return new KS1Output
        {

        };
    }
}
