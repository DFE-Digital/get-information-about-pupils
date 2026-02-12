using DfE.GIAP.Core.Common.Application.ValueObjects;
using DfE.GIAP.Core.MyPupils.Application.Ports;
using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils.Handlers;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.QueryModel;
using DfE.GIAP.Core.MyPupils.Domain.Entities;

namespace DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils;
// TODO this COULD be replaced with a CosmosDb implementation to avoid what it previously used - AzureSearch
internal sealed class AggregatePupilsForMyPupilsApplicationService : IAggregatePupilsForMyPupilsApplicationService
{
    private const int UpnQueryLimit = 4000;
    private readonly IQueryMyPupilsPort _queryMyPupilsPort;
    private readonly IOrderPupilsHandler _orderPupilsHandler;
    private readonly IPaginatePupilsHandler _paginatePupilsHandler;

    public AggregatePupilsForMyPupilsApplicationService(
        IQueryMyPupilsPort queryMyPupilsPort,
        IOrderPupilsHandler orderPupilsHandler,
        IPaginatePupilsHandler paginatePupilsHandler)
    {
        ArgumentNullException.ThrowIfNull(queryMyPupilsPort);
        _queryMyPupilsPort = queryMyPupilsPort;

        ArgumentNullException.ThrowIfNull(orderPupilsHandler);
        _orderPupilsHandler = orderPupilsHandler;

        ArgumentNullException.ThrowIfNull(paginatePupilsHandler);
        _paginatePupilsHandler = paginatePupilsHandler;
    }

    public async Task<IEnumerable<Pupil>> GetPupilsAsync(
        UniquePupilNumbers uniquePupilNumbers,
        MyPupilsQueryModel? query = null,
        CancellationToken ctx = default)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(uniquePupilNumbers.Count, UpnQueryLimit);

        if (uniquePupilNumbers.IsEmpty)
        {
            return [];
        }

        IEnumerable<Pupil> allPupils =
            await _queryMyPupilsPort.QueryAsync(uniquePupilNumbers, ctx);


        // If no query, return ALL results
        if (query is null)
        {
            return allPupils;
        }

        // Order, then paginate

        IEnumerable<Pupil> orderedPupils = _orderPupilsHandler.Order(allPupils, query.Order);

        return _paginatePupilsHandler.PaginatePupils(orderedPupils, query.PaginateOptions);
    }
}
