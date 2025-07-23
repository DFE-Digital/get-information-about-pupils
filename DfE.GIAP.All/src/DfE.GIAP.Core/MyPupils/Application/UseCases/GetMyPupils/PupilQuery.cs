using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Request;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
internal record PupilQuery
{
    public PupilQuery(
        PageNumber pageNumber,
        string? sortField,
        SortDirection sortDirection)
    {

        SortField = (string.IsNullOrEmpty(sortField) ? string.Empty : sortField);
        PageNumber = pageNumber.Value;
        SortDirection = sortDirection;
    }
    public string SortField { get; init; } = "";
    public SortDirection SortDirection { get; init; } = SortDirection.Default;
    public int PageNumber { get; init; } = 1;
    public int PageSize => 20;
    public int FetchCount => PageSize + 1; // Overfetch to enable HasMoreResults

    public static PupilQuery Default => new(
        Request.PageNumber.Page(1),
        sortField: string.Empty,
        sortDirection: SortDirection.Default);
}
