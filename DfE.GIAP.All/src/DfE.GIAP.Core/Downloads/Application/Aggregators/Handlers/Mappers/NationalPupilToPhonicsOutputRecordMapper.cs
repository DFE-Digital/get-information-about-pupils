using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.DownloadOutputs;

namespace DfE.GIAP.Core.Downloads.Application.Aggregators.Handlers.Mappers;

public class NationalPupilToPhonicsOutputRecordMapper : IMapper<NationalPupil, PhonicsOutput>
{
    public PhonicsOutput Map(NationalPupil input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return new PhonicsOutput
        {

        };
    }
}
