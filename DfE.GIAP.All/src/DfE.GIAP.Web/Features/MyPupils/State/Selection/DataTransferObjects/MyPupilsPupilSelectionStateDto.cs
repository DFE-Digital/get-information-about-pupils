namespace DfE.GIAP.Web.Features.MyPupils.State.Selection.DataTransferObjects;

public sealed class MyPupilsPupilSelectionStateDto
{
    public Dictionary<string, bool> PupilUpnToSelectedMap { get; set; } = [];
    public PupilSelectionModeDto State { get; set; } = PupilSelectionModeDto.NotSpecified;
}
