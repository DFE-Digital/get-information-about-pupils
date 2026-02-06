namespace DfE.GIAP.Core.Downloads.Application.Ctf;

public class CtfContext
{
    public bool IsEstablishment { get; set; }

    public string SourceLEA { get; set; } = string.Empty;
    public string SourceEstab { get; set; } = string.Empty;
    public string SourceSchoolName { get; set; } = string.Empty;

    public string DestLEA { get; set; } = string.Empty;
    public string DestEstab { get; set; } = string.Empty;

    public string AcademicYear { get; set; } = string.Empty;
}
