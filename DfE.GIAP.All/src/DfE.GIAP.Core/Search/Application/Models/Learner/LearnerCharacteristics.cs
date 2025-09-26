using System.ComponentModel;
using DfE.GIAP.Core.Common.Domain;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.Search.Application.Models.Learner;

/// <summary>
/// Represents a learner's core characteristics as a value object,
/// including date of birth and gender.
/// </summary>
public sealed class LearnerCharacteristics : ValueObject<LearnerCharacteristics>
{
    /// <summary>
    /// Gets the learner's date of birth.
    /// </summary>
    public DateOfBirth BirthDate { get; }

    /// <summary>
    /// Gets the learner's gender.
    /// </summary>
    public Gender Sex { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LearnerCharacteristics"/> class.
    /// Validates that the date of birth is not null and the gender is a defined enum value.
    /// </summary>
    /// <param name="birthDate">The learner's date of birth.</param>
    /// <param name="gender">The learner's gender.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="birthDate"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="gender"/> is not a valid enum value.</exception>
    public LearnerCharacteristics(DateTime birthDate, Gender gender)
    {
        BirthDate = new DateOfBirth(birthDate) ??
            throw new ArgumentNullException(
                nameof(birthDate), "The learner's date of birth must not be null.");

        if (!Enum.IsDefined(typeof(Gender), gender))
        {
            throw new ArgumentException("The learner's gender field is invalid.");
        }

        Sex = gender;
    }

    /// <summary>
    /// Provides the components that determine equality for this value object.
    /// This method is used by the base <see cref="ValueObject{T}"/> class to
    /// implement value-based equality comparisons.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerable{object}"/> containing the <see cref="BirthDate"/> and <see cref="Sex"/> properties.
    /// These are the defining characteristics of the <see cref="LearnerCharacteristics"/> value object.
    /// </returns>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return BirthDate;
        yield return Sex;
    }

    /// <summary>
    /// Defines the supported gender values for a learner.
    /// </summary>
    public enum Gender
    {
        /// <summary>
        /// Represents a male learner.
        /// </summary>
        [Description("M")]
        Male,

        /// <summary>
        /// Represents a female learner.
        /// </summary>
        [Description("F")]
        Female,

        /// <summary>
        /// Represents a learner who identifies as another gender.
        /// </summary>
        [Description("Unspecified")]
        Other
    }
}
