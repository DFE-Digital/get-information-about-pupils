using DfE.GIAP.Core.Downloads.Application.Ctf.Context;
using DfE.GIAP.Core.Downloads.Application.Ctf.Models;

namespace DfE.GIAP.Core.Downloads.Application.Ctf.Builders;

/// <summary>
/// Defines a builder for creating Common Transfer File (CTF) header objects from a specified context.
/// </summary>
public interface ICtfHeaderBuilder
{
    /// <summary>
    /// Builds a CTF header using the specified context.
    /// </summary>
    /// <param name="context">The context that provides configuration and state information required to construct the CTF header. Cannot be
    /// null.</param>
    /// <returns>A new instance of <see cref="CtfHeader"/> representing the constructed header based on the provided context.</returns>
    CtfHeader Build(ICtfHeaderContext context);
}
