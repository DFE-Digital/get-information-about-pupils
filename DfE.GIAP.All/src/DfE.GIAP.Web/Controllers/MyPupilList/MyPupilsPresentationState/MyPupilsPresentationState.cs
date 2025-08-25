namespace DfE.GIAP.Web.Controllers.MyPupilList.PresentationState;

public record MyPupilsPresentationState(
    int Page,
    string SortBy,
    SortDirection SortDirection);
