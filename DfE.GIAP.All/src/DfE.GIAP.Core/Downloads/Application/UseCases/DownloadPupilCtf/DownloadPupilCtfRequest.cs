using System.Reflection.PortableExecutable;

namespace DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilCtf;

public record DownloadPupilCtfRequest(IEnumerable<string> SelectedPupils) : IUseCaseRequest<DownloadPupilCtfResponse>;

public record DownloadPupilCtfResponse(
    byte[]? FileContents = null,
    string? FileName = null,
    string? ContentType = null);

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
        CtfHeaderContext context = new()
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
