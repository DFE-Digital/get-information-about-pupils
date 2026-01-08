namespace DfE.GIAP.Core.Search.Infrastructure.DataTransferObjects;

/// <summary>
/// Data Transfer Object representing a pupil in the Further Education search index.
/// This model is used to deserialize results returned from Azure Cognitive Search.
/// </summary>
public class LearnerDataTransferObject
{
    /// <summary>
    /// Gets or sets the Unique Learner Number (ULN) assigned to the pupil.
    /// This is a nationally recognized identifier for learners.
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

    /// <summary>
    /// Gets or sets the pupil's sex (e.g., Male, Female).
    /// </summary>
    public string? Sex { get; set; }

    /// <summary>
    /// Gets or sets the pupil's gender (e.g., Male, Female).
    /// </summary>
    public string? Gender { get; set; }

    /// <summary>
    /// Gets or sets the pupil's date of birth.
    /// Nullable to support incomplete or anonymized records.
    /// </summary>
    public DateTime? DOB { get; set; }

    /// <summary>
    /// Gets or sets the name of the Local Authority responsible for the pupil.
    /// </summary>
    public string? LocalAuthority { get; set; }
}
