using Microsoft.Azure.Cosmos.Serialization.HybridRow.Schemas;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Request;
public record OrderPupilsBy
{
    // TODO Expression<Func<T for choosing a field?
    public OrderPupilsBy(
        string field,
        SortDirection direction)
    {
        Field = field ?? string.Empty;
        Direction = direction;
    }

    public string Field { get; }
    public SortDirection Direction { get; }
}
