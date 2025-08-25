namespace DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PupilSelectionState.Dto;

public sealed class PupilSelectionStateDto
{
    public Dictionary<string, bool> PupilUpnToSelectedMap { get; set; } = [];
    public SelectAllPupilsState State { get; set; } = SelectAllPupilsState.NotSpecified;
}
