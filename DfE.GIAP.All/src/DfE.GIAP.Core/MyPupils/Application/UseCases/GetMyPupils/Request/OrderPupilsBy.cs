namespace DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Request;
public record OrderPupilsBy
{
    // TODO Expression<Func<T for choosing a field?
    public OrderPupilsBy(
        string field,
        Direction direction)
    {
        Field = field ?? string.Empty;
        Direction = direction;
    }

    public string Field { get; }
    public Direction Direction { get; }
}

public enum Direction
{
    Ascending,
    Descending,
    Default
}
