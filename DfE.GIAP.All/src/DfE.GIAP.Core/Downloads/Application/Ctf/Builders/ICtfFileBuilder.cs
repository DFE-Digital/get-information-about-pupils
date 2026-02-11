using DfE.GIAP.Core.Downloads.Application.Ctf.Context;
using DfE.GIAP.Core.Downloads.Application.Ctf.Models;

namespace DfE.GIAP.Core.Downloads.Application.Ctf.Builders;

/// <summary>
/// Defines a method for aggregating a CTF file asynchronously based on the specified header context and a collection of
/// selected pupils.
/// </summary>
public interface ICtfFileBuilder
{
    /// <summary>
    /// Asynchronously aggregates pupil data into a single CTF file based on the specified header context and selected
    /// pupils.`
    /// </summary>
    /// <param name="ctfHeaderContext">The context containing CTF header information to be used for the aggregated file. Cannot be null.</param>
    /// <param name="selectedPupils">A collection of unique identifiers representing the pupils to include in the aggregation. Cannot be null or
    /// contain null or empty strings.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a CtfFile object with the aggregated
    /// data for the specified pupils.</returns>
    Task<CtfFile> AggregateFileAsync(ICtfHeaderContext ctfHeaderContext, IEnumerable<string> selectedPupils);
}
