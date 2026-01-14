using System.IO.Compression;
using DfE.GIAP.Core.Downloads.Application.FileExports;

namespace DfE.GIAP.Core.Downloads.Infrastructure.FileExports;

public class ZipArchiveBuilder : IZipArchiveBuilder
{
    public async Task<byte[]> CreateZipAsync(
        Dictionary<string, Func<Stream, Task>> fileWriters)
    {
        using MemoryStream ms = new();

        using (ZipArchive zip = new(ms, ZipArchiveMode.Create, true))
        {
            foreach (KeyValuePair<string, Func<Stream, Task>> kvp in fileWriters)
            {
                ZipArchiveEntry entry = zip.CreateEntry(kvp.Key);
                using Stream entryStream = entry.Open();
                await kvp.Value(entryStream);
            }
        }

        return ms.ToArray();
    }
}
