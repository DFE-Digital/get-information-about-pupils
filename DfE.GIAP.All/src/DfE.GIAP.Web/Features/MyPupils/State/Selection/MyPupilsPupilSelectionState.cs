
using DfE.GIAP.Core.Common.CrossCutting;

namespace DfE.GIAP.Web.Features.MyPupils.State.Selection;

public sealed class MyPupilsPupilSelectionState
{
    // TODO consider requiring UniquePupilNumber on contract
    private readonly Dictionary<string, bool> _pupilsToSelectedMap = [];

    private SelectionState _state = SelectionState.Manual;

    public bool IsAllPupilsSelected => _state == SelectionState.SelectAll;

    public bool IsAllPupilsDeselected => _state == SelectionState.DeselectAll;

    public bool IsAnyPupilSelected => IsAllPupilsSelected || _pupilsToSelectedMap.Values.Any(t => t);

    public IEnumerable<string> CurrentPageOfPupils { get; set; }

    public IReadOnlyDictionary<string, bool> GetPupilsWithSelectionState() => _pupilsToSelectedMap.AsReadOnly();

    public bool IsPupilSelected(string upn)
    {
        return _state switch
        {
            SelectionState.SelectAll => true,
            SelectionState.DeselectAll => false,
            _ => _pupilsToSelectedMap.TryGetValue(upn, out bool selected) && selected
        };

    }

    public void SelectAllPupils()
    {
        _state = SelectionState.SelectAll;

        if (_pupilsToSelectedMap.Keys.Count == 0)
        {
            return;
        }

        UpsertUniquePupilNumberSelectionState(
            _pupilsToSelectedMap.Keys,
            isSelected: true);
    }

    public void DeselectAllPupils()
    {
        _state = SelectionState.DeselectAll;

        if (_pupilsToSelectedMap.Keys.Count == 0)
        {
            return;
        }

        UpsertUniquePupilNumberSelectionState(
            _pupilsToSelectedMap.Keys,
            isSelected: false);
    }

    public void UpsertUniquePupilNumberSelectionState(IEnumerable<string> upns, bool isSelected)
    {
        ArgumentNullException.ThrowIfNull(upns);

        if (!upns.Any())
        {
            throw new ArgumentException("Upns is empty");
        }

        foreach (string upn in upns)
        {
            if (!UniquePupilNumberValidator.Validate(upn))
            {
                throw new ArgumentException("Invalid UPN requested");
            }

            if (IsAllPupilsSelected)
            {
                _pupilsToSelectedMap[upn] = true;
                continue;
            }

            if (IsAllPupilsDeselected)
            {
                _pupilsToSelectedMap[upn] = false;
                continue;
            }

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
