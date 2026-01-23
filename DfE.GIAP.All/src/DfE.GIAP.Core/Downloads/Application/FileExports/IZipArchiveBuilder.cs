namespace DfE.GIAP.Core.Downloads.Application.FileExports;

public interface IZipArchiveBuilder
{
    Task<byte[]> CreateZipAsync(Dictionary<string, Func<Stream, Task>> fileWriters);
}
