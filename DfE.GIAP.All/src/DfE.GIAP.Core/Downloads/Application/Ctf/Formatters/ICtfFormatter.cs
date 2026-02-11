using DfE.GIAP.Core.Downloads.Application.Ctf.Models;

namespace DfE.GIAP.Core.Downloads.Application.Ctf.Formatters;

/// <summary>
/// Defines a formatter that serialises CTF files to a specific media type and writes the formatted output to a stream.
/// </summary>
/// <remarks>Implementations of this interface enable conversion of CTF files into various output formats, such as
/// text or binary representations, for use in different applications or protocols. The formatter exposes its supported
/// media type through the ContentType property.</remarks>
public interface ICtfFormatter
{
    /// <summary>
    /// Gets the media type of the content represented by the current instance.
    /// </summary>
    string ContentType { get; }

    /// <summary>
    /// Asynchronously formats the specified CTF file and writes the formatted output to the provided stream.
    /// </summary>
    /// <param name="file">The CTF file to format. Cannot be null.</param>
    /// <param name="output">The stream to which the formatted output will be written. Must be writable and cannot be null.</param>
    /// <returns>A task that represents the asynchronous formatting operation.</returns>
    Task FormatAsync(CtfFile file, Stream output);
}
