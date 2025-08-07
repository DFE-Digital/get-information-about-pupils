namespace DfE.GIAP.Core.Search.FurtherEducation.Application.UseCases.SearchByFirstnameAndOrSurname.Models;

/// <summary>
/// Represents a pupil in the further education context, identified by ULN and URN,
/// and described by their forename and surname.
/// </summary>
public sealed class FurtherEducationPupil
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FurtherEducationPupil"/> class.
    /// </summary>
    /// <param name="uniqueLearnerNumber">The Unique Learner Number (ULN) assigned to the pupil.</param>
    /// <param name="surname">The pupil's surname.</param>
    /// <param name="forename">The pupil's forename.</param>
    public FurtherEducationPupil(string uniqueLearnerNumber, string surname, string forename)
    {
        UniqueLearnerNumber = uniqueLearnerNumber;
        Surname = surname;
        Forename = forename;
    }

    /// <summary>
    /// Gets the Unique Learner Number (ULN) for the pupil.
    /// </summary>
    public string UniqueLearnerNumber { get; }

    /// <summary>
    /// Gets the pupil's surname.
    /// </summary>
    public string Surname { get; }

    /// <summary>
    /// Gets the pupil's forename.
    /// </summary>
    public string Forename { get; }
}
