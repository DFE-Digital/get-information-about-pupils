namespace DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Request;
public record MyPupilsQueryOptions(OrderPupilsBy Order, PageNumber Page)
{
    internal static MyPupilsQueryOptions Default()
        => new(
            Order: new OrderPupilsBy(
                field: string.Empty,
                SortDirection.Default),
            Page: new PageNumber(1));
}
