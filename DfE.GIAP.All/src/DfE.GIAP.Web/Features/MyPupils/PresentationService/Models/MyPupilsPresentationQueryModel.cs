namespace DfE.GIAP.Web.Features.MyPupils.PresentationService.Models;

public record MyPupilsPresentationQueryModel(
    int Page,
    string SortBy,
    SortDirection SortDirection) // TODO GUARD FOR INVALID STATES
{
    public static MyPupilsPresentationQueryModel CreateDefault() => new(Page: 1, SortBy: string.Empty, SortDirection: SortDirection.Descending);
}
