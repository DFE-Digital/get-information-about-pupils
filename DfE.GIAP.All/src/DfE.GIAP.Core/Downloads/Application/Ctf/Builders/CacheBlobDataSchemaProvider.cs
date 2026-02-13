using DfE.GIAP.Core.Common.Infrastructure.BlobStorage;
using DfE.GIAP.Core.Downloads.Application.Ctf.Models;
using DfE.GIAP.Core.Downloads.Application.Ctf.Options;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace DfE.GIAP.Core.Downloads.Application.Ctf.Builders;

public class CacheBlobDataSchemaProvider : IDataSchemaProvider
{
    private readonly IBlobStorageProvider _blobStorageProvider;
    private readonly TimeSpan _cacheDuration;

    private IReadOnlyList<DataSchemaDefinition>? _cachedSchemas;
    private DateTimeOffset _cacheTimestamp = DateTimeOffset.MinValue;

    private readonly SemaphoreSlim _lock = new(1, 1);

    public CacheBlobDataSchemaProvider(
        IBlobStorageProvider blobStorageProvider,
        IOptions<CtfSchemaCacheOptions> options)
    {
        ArgumentNullException.ThrowIfNull(blobStorageProvider);
        _blobStorageProvider = blobStorageProvider;
        _cacheDuration = options.Value.ToTimeSpan();
    }

    public async Task<IReadOnlyList<DataSchemaDefinition>> GetAllSchemasAsync()
    {
        if (_cachedSchemas is not null && DateTimeOffset.UtcNow - _cacheTimestamp < _cacheDuration)
            return _cachedSchemas;

        await _lock.WaitAsync();
        try
        {
            if (_cachedSchemas is null ||
                DateTimeOffset.UtcNow - _cacheTimestamp >= _cacheDuration)
            {
                _cachedSchemas = await LoadAllSchemasAsync();
                _cacheTimestamp = DateTimeOffset.UtcNow;
            }
        }
        finally
        {
            _lock.Release();
        }

        return _cachedSchemas;
    }

    public async Task<DataSchemaDefinition?> GetSchemaByYearAsync(int year)
    {
        IReadOnlyList<DataSchemaDefinition> schemas = await GetAllSchemasAsync();
        return schemas.FirstOrDefault(s => s.Year == year.ToString());
    }

    private async Task<IReadOnlyList<DataSchemaDefinition>> LoadAllSchemasAsync()
    {
        IEnumerable<BlobItemMetadata> blobs =
            await _blobStorageProvider.ListBlobsWithMetadataAsync("giapdownloads", "CTF");

        IEnumerable<Task<DataSchemaDefinition>> tasks = blobs.Select(async blob =>
        {
            using Stream stream = await _blobStorageProvider
                .DownloadBlobAsStreamAsync("giapdownloads", blob.Name!);

            using StreamReader reader = new(stream);
            string json = await reader.ReadToEndAsync();

            return JsonConvert.DeserializeObject<DataSchemaDefinition>(json)
                   ?? new DataSchemaDefinition();
        });

        return await Task.WhenAll(tasks);
    }
}
