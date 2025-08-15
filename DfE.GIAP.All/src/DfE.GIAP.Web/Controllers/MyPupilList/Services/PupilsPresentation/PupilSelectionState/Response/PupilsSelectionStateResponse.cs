namespace DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PupilSelectionState.Response;

public record PupilsSelectionStateResponse(
    IEnumerable<PupilPresentatationViewModel> Pupils,
    bool isAnyPupilsSelected,
    bool isAllPupilsSelected);
