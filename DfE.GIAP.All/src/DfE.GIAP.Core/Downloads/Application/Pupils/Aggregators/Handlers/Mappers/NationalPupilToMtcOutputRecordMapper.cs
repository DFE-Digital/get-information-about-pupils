using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.DownloadOutputs;

namespace DfE.GIAP.Core.Downloads.Application.Pupils.Aggregators.Handlers.Mappers;

public class NationalPupilToMtcOutputRecordMapper : IMapper<NationalPupil, MTCOutput>
{
    public NationalPupilToMtcOutputRecordMapper()
    {
    }

    public MTCOutput Map(NationalPupil input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return new MTCOutput
        {

        };
    }
}
