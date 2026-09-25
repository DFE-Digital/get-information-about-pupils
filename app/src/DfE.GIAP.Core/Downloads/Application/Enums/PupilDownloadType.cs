namespace DfE.GIAP.Core.Downloads.Application.Enums;

/// <summary>
/// Specifies the types of pupil data available for download in educational data processing operations.
/// </summary>
/// <remarks>Use this enumeration to select the category of pupil information to retrieve, such as National Pupil
/// Database (NPD), Pupil Premium, or Further Education data. The selected value determines the scope and content of the
/// data returned by download operations.</remarks>
public enum PupilDownloadType
{
    NPD,
    PupilPremium,
    FurtherEducation
}
