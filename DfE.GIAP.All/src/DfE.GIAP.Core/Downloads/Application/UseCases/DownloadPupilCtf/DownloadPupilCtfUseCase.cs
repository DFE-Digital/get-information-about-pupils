using DfE.GIAP.Core.Downloads.Application.Ctf;
using DfE.GIAP.Core.Downloads.Application.Ctf.Builders;

namespace DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilCtf;

public class DownloadPupilCtfUseCase : IUseCase<DownloadPupilCtfRequest, DownloadPupilCtfResponse>
{
    private readonly ICtfHeaderBuilder _ctfHeaderBuilder;
    private readonly ICtfPupilBuilder _ctfPupilBuilder;
    private readonly ICtfFormatter _ctfFormatter;

    public DownloadPupilCtfUseCase(
        ICtfHeaderBuilder ctfHeaderBuilder,
        ICtfPupilBuilder ctfPupilBuilder,
        ICtfFormatter ctfFormatter)
    {
        ArgumentNullException.ThrowIfNull(ctfHeaderBuilder);
        ArgumentNullException.ThrowIfNull(ctfPupilBuilder);
        ArgumentNullException.ThrowIfNull(ctfFormatter);
        _ctfHeaderBuilder = ctfHeaderBuilder;
        _ctfPupilBuilder = ctfPupilBuilder;
        _ctfFormatter = ctfFormatter;
    }

    public async Task<DownloadPupilCtfResponse> HandleRequestAsync(DownloadPupilCtfRequest request)
    {
        CtfContext context = new()
        {
            IsEstablishment = false,
            SourceLEA = "SourceLEA",
            SourceEstab = "SourceEstab",
            SourceSchoolName = "SourceSchoolName",
            DestLEA = "DestLEA",
            DestEstab = "DestEstab",
            AcademicYear = "AcademicYear"
        };

        CtfHeader ctfHeader = _ctfHeaderBuilder.Build(context);
        IEnumerable<CtfPupil> ctfPupils = await _ctfPupilBuilder.Build(request.SelectedPupils);

        byte[] ctfFileContents = _ctfFormatter.Format(
            header: ctfHeader,
            pupils: ctfPupils);

        return new DownloadPupilCtfResponse
        {
            FileContents = ctfFileContents,
            FileName = "CTF.xml",
            ContentType = _ctfFormatter.ContentType
        };
    }
}
