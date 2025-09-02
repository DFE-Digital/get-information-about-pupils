using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Command;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.Repositories;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.DataTransferObjects;
using DfE.GIAP.Core.Users.Application;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace DfE.GIAP.Core.MyPupils.Infrastructure.Repositories;
internal sealed class CosmosDbMyPupilsWriteOnlyRepository : IMyPupilsWriteOnlyRepository
{
    private readonly ILogger<CosmosDbMyPupilsWriteOnlyRepository> _logger;
    private readonly ICosmosDbCommandHandler _cosmosDbCommandHandler;
    private readonly IMapper<MyPupilsDocumentDtoMappable, MyPupilsDocumentDto> _mapToDto;

    public CosmosDbMyPupilsWriteOnlyRepository(
        ILogger<CosmosDbMyPupilsWriteOnlyRepository> logger,
        ICosmosDbCommandHandler cosmosDbCommandHandler,
        IMapper<MyPupilsDocumentDtoMappable, MyPupilsDocumentDto> mapToDto)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;

        ArgumentNullException.ThrowIfNull(cosmosDbCommandHandler);
        _cosmosDbCommandHandler = cosmosDbCommandHandler;

        ArgumentNullException.ThrowIfNull(mapToDto);
        _mapToDto = mapToDto;
    }

    public async Task SaveMyPupilsAsync(UserId userId, UniquePupilNumbers updatedMyPupils)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(userId);

            MyPupilsDocumentDto updatedDocument = _mapToDto.Map(
                new MyPupilsDocumentDtoMappable(userId, updatedMyPupils));

            await _cosmosDbCommandHandler.UpsertItemAsync<MyPupilsDocumentDto>(
                item: updatedDocument,
                containerKey: "mypupils",
                partitionKeyValue: userId.Value);
        }
        catch (CosmosException)
        {
            _logger.LogCritical("{method} Error in saving MyPupilsAsync for user: {userId}", nameof(SaveMyPupilsAsync), userId.Value);
            throw;
        }
    }
}

public record MyPupilsDocumentDtoMappable(UserId UserId, UniquePupilNumbers Upns);

internal sealed class MyPupilsDocumentMappableToMyPupilsDocumentDtoMapper : IMapper<MyPupilsDocumentDtoMappable, MyPupilsDocumentDto>
{
    public MyPupilsDocumentDto Map(MyPupilsDocumentDtoMappable input)
    {
        IEnumerable<MyPupilsPupilItemDto> updatedPupils = input.Upns.AsValues()?.Select((upn) => new MyPupilsPupilItemDto()
        {
            UPN = upn
        }) ?? [];

        MyPupilsDto pupilsDto = new()
        {
            Pupils = updatedPupils,
        };

        MyPupilsDocumentDto documentDto = new()
        {
            id = input.UserId.Value,
            MyPupils = pupilsDto
        };

        return documentDto;
    }
}
