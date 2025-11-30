using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Command;
using DfE.GIAP.Core.MyPupils.Application.Repositories;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.Write;
internal sealed class CosmosDbMyPupilsWriteOnlyRepository : IMyPupilsWriteOnlyRepository
{
    private readonly ILogger<CosmosDbMyPupilsWriteOnlyRepository> _logger;
    private readonly ICosmosDbCommandHandler _cosmosDbCommandHandler;
    private readonly IMapper<MyPupilsAggregate, MyPupilsDocumentDto> _mapToDto;

    public CosmosDbMyPupilsWriteOnlyRepository(
        ILogger<CosmosDbMyPupilsWriteOnlyRepository> logger,
        ICosmosDbCommandHandler cosmosDbCommandHandler,
        IMapper<MyPupilsAggregate, MyPupilsDocumentDto> mapToDto)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;

        ArgumentNullException.ThrowIfNull(cosmosDbCommandHandler);
        _cosmosDbCommandHandler = cosmosDbCommandHandler;

        ArgumentNullException.ThrowIfNull(mapToDto);
        _mapToDto = mapToDto;
    }

    public async Task SaveMyPupilsAsync(MyPupilsAggregate myPupils)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(myPupils);

            MyPupilsDocumentDto updatedDocument = _mapToDto.Map(myPupils);

            await _cosmosDbCommandHandler.UpsertItemAsync(
                item: updatedDocument,
                containerKey: "mypupils",
                partitionKeyValue: updatedDocument.id);
        }
        catch (CosmosException)
        {
            _logger.LogCritical("{method} Error in saving MyPupilsAsync for id: {id}", nameof(SaveMyPupilsAsync), myPupils.AggregateId.Value);
            throw;
        }
    }
}
