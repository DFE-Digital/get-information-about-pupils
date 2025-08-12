using DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation;
using DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PupilSelectionState.Response;

namespace DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.Response;

public record GetPupilsForUserFromPresentationStateResponse(
    IEnumerable<PupilPresentatationViewModel> Pupils,
    bool IsAnyPupilsSelected,
    bool IsAllPupilsSelected,
    int Page,
    string SortBy,
    string SortDirection);
