namespace DfE.GIAP.Core.MyPupils.Domain.Services;
public record PupilSelectionDomainCriteria(
    string SortBy,
    SortDirection Direction,
    int Page)
{

    public int Count => 20;
    public static PupilSelectionDomainCriteria Default => new(
        SortBy: string.Empty,
        Page: 1,
        Direction: SortDirection.Ascending);
}
