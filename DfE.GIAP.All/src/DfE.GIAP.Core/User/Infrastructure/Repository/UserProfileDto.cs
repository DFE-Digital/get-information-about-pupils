namespace DfE.GIAP.Core.User.Infrastructure.Repository;
public sealed class UserProfileDto
{
    public string? UserId { get; set; }
    public int DOCTYPE { get; set; }
    public IEnumerable<PupilItemDto> MyPupils 
    {
        get
        {
            IEnumerable<PupilItemDto> pupiList = PupilList?.Select(t => new PupilItemDto()
            {
                Id = Guid.NewGuid(),
                PupilId = t
            }) ?? [];

            return pupiList.Concat(MyPupilList ?? []);
        }
    }

    // TODO data job to merge into a unique list between PupilList and MyPupilList and add a Guid
    public string[] PupilList { get; set; } = [];
    public IEnumerable<PupilItemDto> MyPupilList { get; set; } = [];


    //public string? id { get; set; }
    //public DateTime? CreatedDateTime { get; set; }
    //public DateTime? RecentLoginDateTime { get; set; }
    //public DateTime? PreviousLoginDateTime { get; set; }
    //public DateTime? LatestNewsAccessedDateTime { get; set; }
}
