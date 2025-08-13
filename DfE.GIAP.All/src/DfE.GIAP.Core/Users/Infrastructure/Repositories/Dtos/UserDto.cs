namespace DfE.GIAP.Core.Users.Infrastructure.Repositories.Dtos;
#nullable disable
public sealed class UserDto
{
    public string id { get; set; }
    // TODO data job to merge into a unique list between PupilList and MyPupilList to new property
    public MyPupilsDto MyPupils { get; set; }
    public DateTime LastLoggedIn { get; set; }
}
