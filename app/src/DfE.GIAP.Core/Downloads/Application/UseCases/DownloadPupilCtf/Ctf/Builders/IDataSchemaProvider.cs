using DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilCtf.Ctf.Models;

namespace DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilCtf.Ctf.Builders;

public interface IDataSchemaProvider
{
    Task<IReadOnlyList<DataSchemaDefinition>> GetAllSchemasAsync();
    Task<DataSchemaDefinition?> GetSchemaByYearAsync(int year);
}

