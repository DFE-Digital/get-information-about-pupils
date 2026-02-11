using DfE.GIAP.Core.Downloads.Application.Ctf.Models;

namespace DfE.GIAP.Core.Downloads.Application.Ctf.Builders;

/// <summary>
/// Defines a method for asynchronously constructing a collection of CtfPupil objects based on a set of selected pupil
/// identifiers.
/// </summary>
public interface ICtfPupilBuilder
{
    /// <summary>
    /// Asynchronously builds a collection of CtfPupil objects for the specified pupil identifiers.
    /// </summary>
    /// <param name="selectedPupilIds">A collection of pupil identifiers for which to build CtfPupil objects. Each identifier must be a non-null,
    /// non-empty string.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of CtfPupil objects
    /// corresponding to the specified identifiers.</returns>
    Task<IEnumerable<CtfPupil>> BuildAsync(IEnumerable<string> selectedPupilIds);
}
