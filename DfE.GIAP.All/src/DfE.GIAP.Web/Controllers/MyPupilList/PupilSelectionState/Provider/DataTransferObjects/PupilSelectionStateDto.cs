using DfE.GIAP.Web.Controllers.MyPupilList.PupilSelectionState;

namespace DfE.GIAP.Web.Controllers.MyPupilList.PupilSelectionState.Provider.DataTransferObjects;

public sealed class PupilSelectionStateDto
{
    public Dictionary<string, bool> PupilUpnToSelectedMap { get; set; } = [];
    public SelectAllPupilsState State { get; set; } = SelectAllPupilsState.NotSpecified;
}
