namespace DfE.GIAP.Core.Search.FurtherEducation.Infrastructure.DataTransferObjects;

/// <summary>
/// Data Transfer Object representing a pupil in the further education search index.
/// This class is used to deserialize results from Azure Cognitive Search.
/// </summary>
public class FurtherEducationPupil
{
    /// <summary>
    /// Gets or sets the Unique Learner Number (ULN) assigned to the pupil.
    /// </summary>
    public string? ULN { get; set; }

    /// <summary>
    /// Gets or sets the pupil's surname.
    /// </summary>
    public string? Surname { get; set; }

    /// <summary>
    /// Gets or sets the pupil's forename.
    /// </summary>
    public string? Forename { get; set; }
}
