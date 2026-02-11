namespace DfE.GIAP.Core.Downloads.Application.DataDownloads.FileExports;

public interface IZipArchiveBuilder
{
    Task<byte[]> CreateZipAsync(Dictionary<string, Func<Stream, Task>> fileWriters);
}
