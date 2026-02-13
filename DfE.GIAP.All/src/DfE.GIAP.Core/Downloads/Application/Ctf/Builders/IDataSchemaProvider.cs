using DfE.GIAP.Core.Downloads.Application.Ctf.Models;

namespace DfE.GIAP.Core.Downloads.Application.Ctf.Builders;

public interface IDataSchemaProvider
{
    Task<IReadOnlyList<DataSchemaDefinition>> GetAllSchemasAsync();
    Task<DataSchemaDefinition?> GetSchemaByYearAsync(int year);
}

