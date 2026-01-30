namespace DfE.GIAP.Core.Downloads.Application.Models.DownloadOutputs;

public class KS1OutputRecord
{
    public string? ACADYR { get; set; }
    public string? PUPILMATCHINGREF { get; set; }
    public string? KS1_ID { get; set; }
    public string? UPN { get; set; }
    public string? SURNAME { get; set; }
    public string? FORENAMES { get; set; }
    public string? DOB { get; set; }
    public string? GENDER { get; set; } // Needed for historical KS1 files which are no longer produced.
    public string? SEX { get; set; }
    public string? LA { get; set; }
    public string? LA_9Code { get; set; }
    public string? ESTAB { get; set; }
    public string? LAESTAB { get; set; }
    public string? URN { get; set; }
    public string? ToE_CODE { get; set; }
    public string? MMSCH { get; set; }
    public string? MMSCH2 { get; set; }
    public string? MSCH { get; set; }
    public string? MSCH2 { get; set; }
    public string? MOB1 { get; set; }
    public string? MOB2 { get; set; }
    public string? DISC_READ { get; set; }
    public string? DISC_WRIT { get; set; }
    public string? DISC_MAT { get; set; }
    public string? DISC_SCI { get; set; }
    public string? READ_OUTCOME { get; set; }
    public string? WRIT_OUTCOME { get; set; }
    public string? MATH_OUTCOME { get; set; }
    public string? SCI_OUTCOME { get; set; }
    public string? NPDPUB { get; set; }
    public string? VERSION { get; set; }
}
