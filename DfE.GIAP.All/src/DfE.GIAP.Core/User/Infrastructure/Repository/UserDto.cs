namespace DfE.GIAP.Core.User.Infrastructure.Repository;
# nullable disable
public sealed class UserDto
{
    public string id { get; set; }
    // TODO data job to merge into a unique list between PupilList and MyPupilList
    public MyPupilsDto MyPupils { get; set; }
    //public DateTime? CreatedDateTime { get; set; }
    //public DateTime? RecentLoginDateTime { get; set; }
    //public DateTime? PreviousLoginDateTime { get; set; }
    //public DateTime? LatestNewsAccessedDateTime { get; set; }
}

public sealed class MyPupilsDto
{
    public IEnumerable<MyPupilItemDto> Pupils { get; set; } = [];
}
