namespace DfE.GIAP.Core.User.Infrastructure.Repository;
public sealed class UserProfileDto
{
    //public string? id { get; set; }
    public string? UserId { get; set; }
    public int DOCTYPE { get; set; }
    //public DateTime? CreatedDateTime { get; set; }
    //public DateTime? RecentLoginDateTime { get; set; }
    //public DateTime? PreviousLoginDateTime { get; set; }
    //public DateTime? LatestNewsAccessedDateTime { get; set; }
    public string[] PupilList { get; set; } = [];
    public IEnumerable<UserMyPupilItemDto> MyPupilList { get; set; } = [];
}
