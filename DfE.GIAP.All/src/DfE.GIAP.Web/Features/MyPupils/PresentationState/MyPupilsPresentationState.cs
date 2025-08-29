namespace DfE.GIAP.Web.Features.MyPupils.PresentationState;

public record MyPupilsPresentationState(
    int Page,
    string SortBy,
    SortDirection SortDirection);
