namespace DfE.GIAP.Web.Features.MyPupils.SelectionState.Provider.DataTransferObjects;

public sealed class MyPupilsPupilSelectionStateDto
{
    public Dictionary<string, bool> PupilUpnToSelectedMap { get; set; } = [];
    public PupilSelectionModeDto State { get; set; } = PupilSelectionModeDto.NotSpecified;
}
