using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.DownloadOutputs;

namespace DfE.GIAP.Core.Downloads.Application.Pupils.Aggregators.Handlers.Mappers;

public class NationalPupilToCensusAutumnOutputRecordMapper : IMapper<NationalPupil, CensusAutumnOutput>
{
    public CensusAutumnOutput Map(NationalPupil input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return new CensusAutumnOutput
        {

        };
    }
}
