using DfE.GIAP.Core.Downloads.Application.Ctf.Builders;
using DfE.GIAP.Core.Downloads.Application.Ctf.Context;
using DfE.GIAP.Core.Downloads.Application.Ctf.Formatters;
using DfE.GIAP.Core.Downloads.Application.Ctf.Models;

namespace DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilCtf;

public class DownloadPupilCtfUseCase : IUseCase<DownloadPupilCtfRequest, DownloadPupilCtfResponse>
{
    private readonly ICtfFileBuilder _ctfFileAggregator;
    private readonly ICtfFormatter _ctfFormatter;

    public DownloadPupilCtfUseCase(
        ICtfFileBuilder ctfFileAggregator,
        ICtfFormatter ctfFormatter)
    {
        ArgumentNullException.ThrowIfNull(ctfFileAggregator);
        ArgumentNullException.ThrowIfNull(ctfFormatter);
        _ctfFileAggregator = ctfFileAggregator;
        _ctfFormatter = ctfFormatter;
    }

    public async Task<DownloadPupilCtfResponse> HandleRequestAsync(DownloadPupilCtfRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.SelectedPupils is null || !request.SelectedPupils.Any())
            throw new ArgumentException("At least one pupil must be selected.", nameof(request.SelectedPupils));

        CtfFile ctfFile = await _ctfFileAggregator.AggregateFileAsync(new ClaimsCtfHeaderContext()
        {
            IsEstablishment = request.IsEstablishment,
            LocalAuthorityNumber = request.LocalAuthoriyNumber,
            EstablishedNumber = request.EstablishmentNumber
        }, request.SelectedPupils);

        MemoryStream stream = new();
        await _ctfFormatter.FormatAsync(ctfFile, stream);
        stream.Position = 0;

        return new DownloadPupilCtfResponse(stream, "CTF.xml", _ctfFormatter.ContentType);
    }
}
