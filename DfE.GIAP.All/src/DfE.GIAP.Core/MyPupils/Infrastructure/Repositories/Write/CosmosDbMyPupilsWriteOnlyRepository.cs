using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Command;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.Repositories;
using DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.DataTransferObjects;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.Write;
internal sealed class CosmosDbMyPupilsWriteOnlyRepository : IMyPupilsWriteOnlyRepository
{
    private readonly ILogger<CosmosDbMyPupilsWriteOnlyRepository> _logger;
    private readonly ICosmosDbCommandHandler _cosmosDbCommandHandler;
    private readonly IMapper<Domain.AggregateRoot.MyPupils, MyPupilsDocumentDto> _mapToDto;

    public CosmosDbMyPupilsWriteOnlyRepository(
        ILogger<CosmosDbMyPupilsWriteOnlyRepository> logger,
        ICosmosDbCommandHandler cosmosDbCommandHandler,
        IMapper<Domain.AggregateRoot.MyPupils, MyPupilsDocumentDto> mapToDto)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;

        ArgumentNullException.ThrowIfNull(cosmosDbCommandHandler);
        _cosmosDbCommandHandler = cosmosDbCommandHandler;

        ArgumentNullException.ThrowIfNull(mapToDto);
        _mapToDto = mapToDto;
    }

    public async Task SaveMyPupilsAsync(Domain.AggregateRoot.MyPupils myPupils)
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
