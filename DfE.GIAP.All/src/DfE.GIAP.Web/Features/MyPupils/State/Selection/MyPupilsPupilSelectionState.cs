using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using NuGet.Packaging;

namespace DfE.GIAP.Web.Features.MyPupils.State.Selection;

public sealed class MyPupilsPupilSelectionState
{
    private readonly Dictionary<UniquePupilNumber, bool> _pupilsToSelectedMap = [];

    private SelectionState _state = SelectionState.Manual;

    public bool IsAllPupilsSelected => _state == SelectionState.SelectAll;

    public bool IsAllPupilsDeselected => _state == SelectionState.DeselectAll;

    public bool IsAnyPupilSelected => IsAllPupilsSelected || _pupilsToSelectedMap.Values.Any(t => t);

    public static MyPupilsPupilSelectionState CreateDefault() => new();

    public IReadOnlyDictionary<UniquePupilNumber, bool> GetPupilsWithSelectionState() => _pupilsToSelectedMap.AsReadOnly();

    // Note: A pupil under SelectAll or DeselectAll, may have a manual selection applied, so this is a single "apply at the point of SelectAll/Deselect".
    public bool IsPupilSelected(UniquePupilNumber upn) => _pupilsToSelectedMap.TryGetValue(upn, out bool selected) && selected;

    public void SelectAllPupils()
    {
        _state = SelectionState.SelectAll;

        UpsertPupilSelectionState(
            _pupilsToSelectedMap.Keys,
            isSelected: true);
    }

    public void DeselectAllPupils()
    {
        _state = SelectionState.DeselectAll;

        UpsertPupilSelectionState(
            _pupilsToSelectedMap.Keys,
            isSelected: false);
    }

    public void UpsertPupilSelectionState(IEnumerable<UniquePupilNumber> upns, bool isSelected)
    {
        ArgumentNullException.ThrowIfNull(upns);

        foreach (UniquePupilNumber upn in upns)
        {
            _pupilsToSelectedMap[upn] = isSelected;
        }
    }

    public void ResetState()
    {
        _pupilsToSelectedMap.Clear();
        _state = SelectionState.Manual;
    }

    private enum SelectionState
    {
        SelectAll,
        DeselectAll,
        Manual
    }
}
