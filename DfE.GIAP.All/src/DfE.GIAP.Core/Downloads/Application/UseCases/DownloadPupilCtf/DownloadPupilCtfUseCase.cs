using DfE.GIAP.Core.Downloads.Application.Aggregators;
using DfE.GIAP.Core.Downloads.Application.Ctf;
using DfE.GIAP.Core.Downloads.Application.FileExports;

namespace DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilCtf;

public class DownloadPupilCtfUseCase : IUseCase<DownloadPupilCtfRequest, DownloadPupilCtfResponse>
{
    private readonly ICtfFileAggregator _ctfFileAggregator;
    private readonly ICtfFormatter _ctfFormatter;

    public DownloadPupilCtfUseCase(
        ICtfFileAggregator ctfFileAggregator,
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
