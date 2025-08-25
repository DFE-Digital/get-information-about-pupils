namespace DfE.GIAP.Web.Controllers.MyPupilList.Handlers.GetPupilsForUserFromPresentationState.Response;

public record GetPupilsForUserFromPresentationStateResponse(
    IEnumerable<PupilPresentationViewModel> Pupils,
    bool IsAnyPupilsSelected,
    bool IsAllPupilsSelected,
    int Page,
    string SortBy,
    string SortDirection);
