using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Query;
using DfE.GIAP.Core.MyPupils.Application.UseCase.GetMyPupils.Handler;
using DfE.GIAP.Core.MyPupils.Domain;
using DfE.GIAP.Core.MyPupils.Domain.Entities;

namespace DfE.GIAP.Core.Common.Domain.User.Repository;
internal sealed class CosmosDbUserReadOnlyRepository : IUserReadOnlyRepository
{
    private readonly ICosmosDbQueryHandler _cosmosDbQueryHandler;
    private readonly IGetMyPupilsHandler _getMyPupilsHandler;

    public CosmosDbUserReadOnlyRepository(
        ICosmosDbQueryHandler cosmosDbQueryHandler,
        IGetMyPupilsHandler getMyPupilsHandler)
    {
        _cosmosDbQueryHandler = cosmosDbQueryHandler;
        _getMyPupilsHandler = getMyPupilsHandler;
    }

    public async Task<UserAggregateRoot> GetByIdAsync(
        UserIdentifier id,
        MyPupilsAuthorisationContext myPupilsAuthorisationContext,
        CancellationToken ctx = default)
    {
        // Old query for UserProfile impl
        DateTime currentDateTime = DateTime.UtcNow;

        string query = $"SELECT * FROM u " +
                    $"WHERE u.DOCTYPE = 10 AND u.UserId = '{id.Value}' " +
                    $"ORDER BY u._ts DESC " +
                    $"OFFSET 0 LIMIT 1";


        IEnumerable<UserProfileDto> results = await _cosmosDbQueryHandler.ReadItemsAsync<UserProfileDto>(
            containerKey: "application-data",
            query,
            ctx);

        if (!results.Any())
        {
            throw new ArgumentException($"Unable to find User by identifier {id.Value}");
        }

        UserProfileDto user = results.First();

        IEnumerable<string> userPupilUrns =
            user.MyPupilList
                .Select(t => t.PupilId!)
                .Where(t => !string.IsNullOrEmpty(t))
                .Concat(user.PupilList ?? []) // TODO understand why this is joined, does PupilList even exist in persistence
                .Distinct(); // TODO should we be making these distinct, is Web doing this currently

        //UserProfileDto previouslyReturnedDto = new()
        //{
        //    UserId = user.UserId,
        //    id = user.id,
        //    CreatedDateTime = user.CreatedDateTime,
        //    PreviousLoginDateTime = user.PreviousLoginDateTime,
        //    RecentLoginDateTime = user.RecentLoginDateTime,
        //    PupilList = user.PupilList ?? [],
        //    MyPupilList = newMyPupilList,
        //    LatestNewsAccessedDateTime = user.LatestNewsAccessedDateTime
        //};

        IEnumerable<MyPupil> pupils = await _getMyPupilsHandler.HandleAsync(
            myPupilsAuthorisationContext,
            userPupilUrns,
            ctx);

        return new UserAggregateRoot(
            id,
            pupils);
    }

    public sealed class UserProfileDto
    {
        public string? id { get; set; }
        public string? UserId { get; set; }
        public DateTime? CreatedDateTime { get; set; }
        public DateTime? RecentLoginDateTime { get; set; }
        public DateTime? PreviousLoginDateTime { get; set; }
        public DateTime? LatestNewsAccessedDateTime { get; set; }
        public string[] PupilList { get; set; } = [];
        public IEnumerable<UserMyPupilItemDto> MyPupilList { get; set; } = [];
        public int DOCTYPE { get; set; }
    }


    public class UserMyPupilItemDto
    {
        public string? PupilId { get; set; }
        public bool IsMasked { get; set; } // Do we still need this given Masked is evaluated by the Domain.
    }
}
