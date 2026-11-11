using DfE.GIAP.Core.Common.Application.ValueObjects;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.Models;
using DfE.GIAP.Web.Features.Search.LegacyModels.Learner;

namespace DfE.GIAP.Web.Features.Search.FurtherEducation;

/// <summary>
/// Maps a domain-level <see cref="FurtherEducationLearner"/> entity
/// into a UI-facing <see cref="Learner"/> view model.
/// Used in text-based search results for Further Education learners.
/// </summary>
public sealed class FurtherEducationLearnerToLearnerMapper :
    IMapper<FurtherEducationLearner, Learner>
{
    /// <summary>
    /// Converts a <see cref="FurtherEducationLearner"/>
    /// into a <see cref="Learner"/> view model.
    /// This bridges domain-layer entities with UI-facing representations,
    /// enabling clean separation between business logic and presentation.
    /// </summary>
    /// <param name="input">The domain learner entity to map.</param>
    /// <returns>A populated <see cref="Learner"/> view model.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="input"/> is null to prevent null dereferencing.
    /// </exception>
    public Learner Map(FurtherEducationLearner input)
    {
        // Defensive null check to ensure safe mapping
        ArgumentNullException.ThrowIfNull(input);

        // Instantiate and populate the view model using domain data
        Learner learner = new()
        {
            // Maps the Unique Learner Number (ULN) as both ID and LearnerNumber
            Id = input.Identifier.UniqueLearnerNumber.ToString(),
            LearnerNumber = input.Identifier.UniqueLearnerNumber.ToString(),

            // Maps name components from domain to view model
            Surname = input.Name.Surname,
            Forename = input.Name.FirstName,

            // Converts sex enum to human-readable string using description meta-data
            Sex = input.Characteristics.Sex.ToString(),

            // Maps birth date directly; assumes implicit conversion from value object to DateTime
            DOB = input.Characteristics.BirthDate
        };

        return learner;
    }
}
