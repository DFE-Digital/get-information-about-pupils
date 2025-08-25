namespace DfE.GIAP.Web.Controllers.MyPupilList.Services.Presentation.PupilSelectionState.Provider.DataTransferObjects;

public sealed class PupilSelectionStateDto
{
    public Dictionary<string, bool> PupilUpnToSelectedMap { get; set; } = [];
    public SelectAllPupilsState State { get; set; } = SelectAllPupilsState.NotSpecified;
}
