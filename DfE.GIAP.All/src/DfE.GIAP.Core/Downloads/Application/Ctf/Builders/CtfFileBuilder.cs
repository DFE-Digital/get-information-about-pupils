using DfE.GIAP.Core.Downloads.Application.Ctf.Context;
using DfE.GIAP.Core.Downloads.Application.Ctf.Models;

namespace DfE.GIAP.Core.Downloads.Application.Ctf.Builders;

public class CtfFileBuilder : ICtfFileBuilder
{
    private readonly ICtfHeaderBuilder _ctfHeaderHandler;
    private readonly ICtfPupilBuilder _ctfPupilHandler;

    public CtfFileBuilder(
        ICtfHeaderBuilder ctfHeaderBuilder,
        ICtfPupilBuilder ctfPupilBuilder)
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
