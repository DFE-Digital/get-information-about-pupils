using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Search.Application.Models.Learner;
using DfE.GIAP.Core.Search.Application.Models.Learner.FurtherEducation;
using DfE.GIAP.Core.Search.Infrastructure.DataTransferObjects;
using static DfE.GIAP.Core.Search.Application.Models.Learner.LearnerCharacteristics;

namespace DfE.GIAP.Core.Search.Infrastructure.Mappers;

/// <summary>
/// Maps a <see cref="FurtherEducationLearnerDataTransferObject"/> data transfer object
/// into a domain-level <see cref="FurtherEducationLearner"/> model.
/// </summary>
public sealed class FurtherEducationSearchResultToLearnerMapper : IMapper<FurtherEducationLearnerDataTransferObject, FurtherEducationLearner>
{
    /// <summary>
    /// Converts a <see cref="FurtherEducationLearnerDataTransferObject"/> into a <see cref="FurtherEducationLearner"/>.
    /// </summary>
    /// <param name="input">The DTO representing a Further Education pupil.</param>
    /// <returns>A mapped domain model representing the learner.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="input"/> contains null or empty required fields.
    /// </exception>
    public FurtherEducationLearner Map(FurtherEducationLearnerDataTransferObject input)
    {
        // Defensive null checks for required fields
        ArgumentNullException.ThrowIfNull(input);
        ArgumentException.ThrowIfNullOrEmpty(input.ULN);
        ArgumentException.ThrowIfNullOrEmpty(input.Forename);
        ArgumentException.ThrowIfNullOrEmpty(input.Surname);
        ArgumentNullException.ThrowIfNull(input.DOB);

        // Construct domain model using validated input
        return new FurtherEducationLearner(
            new FurtherEducationLearnerIdentifier(input.ULN),
            new LearnerName(input.Forename, input.Surname),
            new LearnerCharacteristics(
                input.DOB.Value,
                ParseGender(input.Sex, input.Gender) // Handles nulls and unknowns gracefully
            )
        );
    }

    /// <summary>
    /// Converts a string representation like "M", "F", or "O" to a <see cref="Gender"/> enum value.
    /// Defaults to <see cref="Gender.Other"/> if input is null, empty, or unrecognized.
    /// </summary>
    /// <param name="sex">The input string (e.g., "M", "F", "O").</param>
    /// /// <param name="gender">The input string (e.g., "M", "F", "O").</param>
    /// <returns>The corresponding <see cref="Gender"/> enum value.</returns>
    private static Gender ParseGender(string? sex, string? gender)
    {
        // Return 'Other' if input is null, whitespace, or doesn't match known codes
        if (string.IsNullOrWhiteSpace(sex))
        {
            sex = gender;
        }

        return sex?.Trim().ToUpperInvariant() switch
        {
            "M" => Gender.Male,
            "F" => Gender.Female,
            "O" => Gender.Other,
            _ => Gender.Other // fall-back for unrecognized values
        };
    }
}
