using System.ComponentModel;
using System.Reflection;

namespace DfE.GIAP.Core.Search.Application.Models.Learner;
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

public static class GenderConverterExtensions
{
    /// <summary>
    /// Converts a <see cref="Gender"/> enum value into a human-readable string.
    /// If the enum field has a <see cref="DescriptionAttribute"/>, that value is used;
    /// otherwise, the enum name is returned as a fall-back.
    /// </summary>
    /// <param name="gender">The gender enum value to convert.</param>
    /// <returns>A string description of the gender, suitable for UI display.</returns>
    public static string MapSexDescription(this Gender gender)
    {
        // Use reflection to retrieve the DescriptionAttribute from the enum field
        DescriptionAttribute? descriptionAttribute =
            gender.GetType()                                // Gets the enum type (Gender)
                .GetField(gender.ToString())?               // Retrieves the field info for the enum value
                .GetCustomAttribute<DescriptionAttribute>();// Extracts the DescriptionAttribute if present

        // Return the description if available; otherwise fall-back to enum name
        return descriptionAttribute?.Description ?? gender.ToString();
    }
}
