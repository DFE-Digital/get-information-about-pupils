using DfE.GIAP.Core.Downloads.Application.Ctf.Models;

namespace DfE.GIAP.Core.Downloads.Application.Ctf.Formatters;

/// <summary>
/// Defines a formatter that serializes a CTF (Common Transfer File) file to a specific content type.
/// </summary>
/// <remarks>Implementations of this interface provide formatting logic for converting CTF files into various
/// output formats, such as binary or text representations. The formatter's content type indicates the MIME type of the
/// formatted output.</remarks>
public interface ICtfFormatter
{
    /// <summary>
    /// Gets the media type of the content represented by the current instance.
    /// </summary>
    string ContentType { get; }

    /// <summary>
    /// Formats the specified CTF file and returns its serialized binary representation.
    /// </summary>
    /// <param name="ctfFile">The CTF file to format. Cannot be null.</param>
    /// <returns>A byte array containing the formatted binary data of the specified CTF file.</returns>
    byte[] Format(CtfFile ctfFile);
}
