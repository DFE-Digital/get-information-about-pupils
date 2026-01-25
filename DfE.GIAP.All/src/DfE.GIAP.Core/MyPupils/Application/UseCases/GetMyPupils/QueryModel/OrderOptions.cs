namespace DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.QueryModel;

public record OrderOptions
{
    public OrderOptions(string field, string direction) // TODO valid fields from options
    {
        Field = string.IsNullOrWhiteSpace(field) ? string.Empty : field;

        Direction =
            string.IsNullOrWhiteSpace(direction) ?
                OrderDirection.NoneOrUnknown :
                    direction.ToLowerInvariant() switch
                    {
                        "asc" => OrderDirection.Ascending,
                        "desc" => OrderDirection.Descending,
                        _ => OrderDirection.NoneOrUnknown
                    };
    }

    public string Field { get; }
    public OrderDirection Direction { get; }
    public static OrderOptions Default() => new(string.Empty, string.Empty);
}
