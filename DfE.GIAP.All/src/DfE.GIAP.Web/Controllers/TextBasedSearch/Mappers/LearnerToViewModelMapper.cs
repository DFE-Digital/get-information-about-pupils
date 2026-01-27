using System.ComponentModel;
using System.Reflection;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Search.Application.Models.Learner.FurtherEducation;
using DfE.GIAP.Domain.Search.Learner;
using static DfE.GIAP.Core.Search.Application.Models.Learner.LearnerCharacteristics;

namespace DfE.GIAP.Web.Controllers.TextBasedSearch.Mappers;

/// <summary>
/// Maps a domain-level <see cref="Core.Search.Application.Models.Learner.FurtherEducation.FurtherEducationLearner"/> entity
/// into a UI-facing <see cref="Learner"/> view model.
/// Used in text-based search results for Further Education learners.
/// </summary>
public sealed class LearnerToViewModelMapper :
    IMapper<FurtherEducationLearner, Learner>
{
    /// <summary>
    /// Converts a <see cref="Core.Search.Application.Models.Learner.FurtherEducation.FurtherEducationLearner"/>
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
            Sex = MapSexDescription(input.Characteristics.Sex),

            // Maps birth date directly; assumes implicit conversion from value object to DateTime
            DOB = input.Characteristics.BirthDate
        };

        return learner;
    }

    /// <summary>
    /// Converts a <see cref="Gender"/> enum value into a human-readable string.
    /// If the enum field has a <see cref="DescriptionAttribute"/>, that value is used;
    /// otherwise, the enum name is returned as a fall-back.
    /// </summary>
    /// <param name="gender">The gender enum value to convert.</param>
    /// <returns>A string description of the gender, suitable for UI display.</returns>
    private static string MapSexDescription(Gender gender)
    {
        // Use reflection to retrieve the DescriptionAttribute from the enum field
        DescriptionAttribute descriptionAttribute =
            gender.GetType()                                // Gets the enum type (Gender)
                .GetField(gender.ToString())?               // Retrieves the field info for the enum value
                .GetCustomAttribute<DescriptionAttribute>();// Extracts the DescriptionAttribute if present

        // Return the description if available; otherwise fall-back to enum name
        return descriptionAttribute?.Description ?? gender.ToString();
    }
}
