namespace DfE.GIAP.Core.User.Infrastructure.Repository.Dtos;
public sealed class MyPupilsDto
{
    public IEnumerable<MyPupilsItemDto> Pupils { get; set; } = [];
}
