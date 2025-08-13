using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Search.FurtherEducation.Application.UseCases.SearchByFirstnameAndOrSurname.Models;
using DfE.GIAP.Domain.Search.Learner;

namespace DfE.GIAP.Web.Controllers.TextBasedSearch.Mappers;

/// <summary>
/// Maps a domain-level <see cref="FurtherEducationLearner"/> entity
/// into a UI-facing <see cref="Learner"/> view model.
/// Used in text-based search results for Further Education learners.
/// </summary>
public sealed class FurtherEducationLearnerToViewModelMapper : IMapper<FurtherEducationLearner, Learner>
{
    /// <summary>
    /// Converts a <see cref="FurtherEducationLearner"/> into a <see cref="Learner"/> view model.
    /// </summary>
    /// <param name="input">The domain learner entity to map.</param>
    /// <returns>A populated <see cref="Learner"/> view model.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null.</exception>
    public Learner Map(FurtherEducationLearner input)
    {
        ArgumentNullException.ThrowIfNull(input);

        Learner learner = new()
        {
            // Maps the ULN as both ID and LearnerNumber
            Id = input.Identifier.UniqueLearnerNumber.ToString(),
            LearnerNumber = input.Identifier.UniqueLearnerNumber.ToString(),
            // Maps name components
            Surname = input.Name.Surname,
            Forename = input.Name.FirstName,
            // Maps gender enum to string
            Gender = input.Characteristics.Sex.ToString(),
            // Implicitly converts DateOfBirth value object to DateTime
            DOB = input.Characteristics.BirthDate
        };

        return learner;
    }
}
