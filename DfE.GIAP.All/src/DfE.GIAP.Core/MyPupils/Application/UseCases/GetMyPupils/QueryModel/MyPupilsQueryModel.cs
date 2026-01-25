namespace DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.QueryModel;

public record MyPupilsQueryModel
{
    public MyPupilsQueryModel(
        int pageNumber,
        int size,
        (string? field, string? direction)? orderBy)
    {
        Order = new OrderOptions(
            orderBy?.field ?? string.Empty,
            orderBy?.direction ?? string.Empty);

        PaginateOptions = new(pageNumber, size);
    }

    public PaginationOptions PaginateOptions { get; }
    public OrderOptions Order { get; }

    public static MyPupilsQueryModel CreateDefault() => new(
        pageNumber: 1,
        size: 20,
        (string.Empty, string.Empty));
}
