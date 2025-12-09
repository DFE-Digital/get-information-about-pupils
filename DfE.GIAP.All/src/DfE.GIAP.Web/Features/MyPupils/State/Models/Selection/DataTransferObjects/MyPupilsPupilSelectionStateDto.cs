namespace DfE.GIAP.Web.Features.MyPupils.State.Models.Selection.DataTransferObjects;


public sealed class MyPupilsPupilSelectionStateDto
{
    // Explicit mode indicator (required to reconstruct the state)
    public SelectionMode Mode { get; init; }

    // When Mode == None: holds explicit selections.
    // When Mode == All : this SHOULD be empty; we use DeselectionExceptions instead.
    public List<string> ExplicitSelections { get; init; } = [];

    // When Mode == All: holds exceptions (UPNs explicitly deselected).
    // When Mode == None: this SHOULD be empty.
    public List<string> DeselectionExceptions { get; init; } = [];
}
