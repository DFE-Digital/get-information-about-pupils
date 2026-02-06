namespace DfE.GIAP.Core.Downloads.Application.Ctf;

public class CtfFile
{
    public CtfHeader Header { get; set; } = new();
    public List<CtfPupil> Pupils { get; set; } = new();
}

public class CtfHeader
{
    public string DocumentName { get; set; } = "Common Transfer File";
    public string CtfVersion { get; set; } = "25.0";
    public DateTime DateTime { get; set; } = DateTime.UtcNow;
    public string DocumentQualifier { get; set; } = "partial";
    public string DataDescriptor { get; set; } = string.Empty;
    public string SupplierId { get; set; } = "GIAP";

    public CtfSchoolInfo SourceSchool { get; set; } = new();
    public CtfSchoolInfo DestSchool { get; set; } = new();
}

public class CtfSchoolInfo
{
    public string LEA { get; set; } = string.Empty;
    public string Estab { get; set; } = string.Empty;
    public string? SchoolName { get; set; }
    public string? AcademicYear { get; set; }
}

public class CtfPupil
{
    public string UPN { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Forename { get; set; } = string.Empty;
    public string DOB { get; set; } = string.Empty; // Parse as: yyyy-MM-dd
    public string? Sex { get; set; }

    public List<CtfKeyStageAssessment> Assessments { get; set; } = new();
}

// Either: EYFSP/Phonics & KS1/KS2/KS2 MTC
public class CtfKeyStageAssessment
{
    public string Stage { get; set; } = string.Empty;
    public string Locale { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Component { get; set; } = string.Empty;
    public string ResultStatus { get; set; } = string.Empty;
    public string ResultQualifier { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
}

