using DfE.GIAP.Core.Downloads.Application.Aggregators.Handlers;
using DfE.GIAP.Core.Downloads.Application.Ctf;

namespace DfE.GIAP.Core.Downloads.Application.Aggregators;

public interface ICtfFileAggregator
{
    Task<CtfFile> AggregateFileAsync(ICtfHeaderContext ctfHeaderContext, IEnumerable<string> selectedPupils);
}

public class CtfFileAggregator : ICtfFileAggregator
{
    private readonly ICtfHeaderHandler _ctfHeaderHandler;
    private readonly ICtfPupilHandler _ctfPupilHandler;

    public CtfFileAggregator(
        ICtfHeaderHandler ctfHeaderBuilder,
        ICtfPupilHandler ctfPupilBuilder)
    {
        ArgumentNullException.ThrowIfNull(ctfHeaderBuilder);
        ArgumentNullException.ThrowIfNull(ctfPupilBuilder);
        _ctfHeaderHandler = ctfHeaderBuilder;
        _ctfPupilHandler = ctfPupilBuilder;
    }

    public async Task<CtfFile> AggregateFileAsync(
        ICtfHeaderContext ctfHeaderContext,
        IEnumerable<string> selectedPupils)
    {
        CtfHeader headerCtf = _ctfHeaderHandler.Build(ctfHeaderContext);
        IEnumerable<CtfPupil> pupilsCtf = await _ctfPupilHandler.BuildAsync(selectedPupils);

        return new(headerCtf, pupilsCtf);
    }
}
