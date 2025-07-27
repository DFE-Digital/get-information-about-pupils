namespace DfE.GIAP.Core.User.Infrastructure.Repository;
# nullable disable
public sealed class UserDto
{
    public string id { get; set; }
    public IEnumerable<MyPupilItemDto> MyPupils { get; set; }
    //{
    //    get
    //    {
    //        IEnumerable<MyPupilItemDto> pupiList = PupilList?.Select(t => new MyPupilItemDto()
    //        {
    //            Id = Guid.NewGuid(),
    //            PupilId = t
    //        }) ?? [];

    //        return pupiList.Concat(MyPupilList ?? []);
    //    }
    //}

    // TODO data job to merge into a unique list between PupilList and MyPupilList and add a Guid
    public string[] PupilList { get; set; } = [];
    public IEnumerable<MyPupilItemDto> MyPupilList { get; set; } = [];


    //public string? id { get; set; }
    //public DateTime? CreatedDateTime { get; set; }
    //public DateTime? RecentLoginDateTime { get; set; }
    //public DateTime? PreviousLoginDateTime { get; set; }
    //public DateTime? LatestNewsAccessedDateTime { get; set; }
}
