namespace DfE.GIAP.Web.Features.MyPupils.State.Models.Presentation;

public record MyPupilsPresentationState(
    int Page,
    string SortBy,
    SortDirection SortDirection) // TODO GUARD FOR INVALID STATES
{
    public static MyPupilsPresentationState CreateDefault() => new(Page: 1, SortBy: string.Empty, SortDirection: SortDirection.Descending);
}
