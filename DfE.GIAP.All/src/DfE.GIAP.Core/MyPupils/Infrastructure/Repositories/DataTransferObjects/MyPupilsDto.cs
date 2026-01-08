namespace DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.DataTransferObjects;
public sealed class MyPupilsDto
{
    public IEnumerable<MyPupilsPupilItemDto> Pupils { get; set; } = [];
}
