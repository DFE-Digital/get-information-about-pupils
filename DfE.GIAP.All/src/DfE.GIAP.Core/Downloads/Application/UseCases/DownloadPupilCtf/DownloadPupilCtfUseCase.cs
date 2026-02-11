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
        CtfFile ctfFile = await _ctfFileAggregator.AggregateFileAsync(new ClaimsCtfHeaderContext()
        {
            IsEstablishment = request.IsEstablishment,
            LocalAuthorityNumber = request.LocalAuthoriyNumber,
            EstablishedNumber = request.EstablishmentNumber
        }, request.SelectedPupils);

        byte[] ctfFileContents = _ctfFormatter.Format(ctfFile);

        return new DownloadPupilCtfResponse
        {
            FileContents = ctfFileContents,
            FileName = "CTF.xml",
            ContentType = _ctfFormatter.ContentType
        }; 
    }
}
