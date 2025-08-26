using DfE.GIAP.Web.Controllers.MyPupilList.PupilSelectionState;

namespace DfE.GIAP.Web.Controllers.MyPupilList.PupilSelectionState.Provider.DataTransferObjects;

public sealed class MyPupilsPupilSelectionStateDto
{
    public Dictionary<string, bool> PupilUpnToSelectedMap { get; set; } = [];
    public PupilSelectionModeDto State { get; set; } = PupilSelectionModeDto.NotSpecified;
}
