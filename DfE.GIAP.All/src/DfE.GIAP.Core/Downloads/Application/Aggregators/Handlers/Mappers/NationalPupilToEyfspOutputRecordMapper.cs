using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.DownloadOutputs;

namespace DfE.GIAP.Core.Downloads.Application.Aggregators.Handlers.Mappers;

public class NationalPupilToEyfspOutputRecordMapper : IMapper<NationalPupil, EYFSPOutput>
{
    public EYFSPOutput Map(NationalPupil input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return new EYFSPOutput
        {

        };
    }
}
