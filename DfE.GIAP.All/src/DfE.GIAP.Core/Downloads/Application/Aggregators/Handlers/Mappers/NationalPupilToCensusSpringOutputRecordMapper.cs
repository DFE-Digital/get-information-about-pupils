using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.DownloadOutputs;

namespace DfE.GIAP.Core.Downloads.Application.Aggregators.Handlers.Mappers;

public class NationalPupilToCensusSpringOutputRecordMapper : IMapper<NationalPupil, CensusSpringOutput>
{
    public CensusSpringOutput Map(NationalPupil input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return new CensusSpringOutput
        {

        };
    }
}
