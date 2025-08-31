using DfE.GIAP.Core.Common.CrossCutting;

namespace DfE.GIAP.Web.Features.MyPupils.SelectionState;

public sealed class MyPupilsPupilSelectionState
{
    private readonly Dictionary<string, bool> _pupilsToSelectedMap = [];
    private SelectAllPupilsState _state = SelectAllPupilsState.NotSpecified;
    public bool IsAllPupilsSelected => _state == SelectAllPupilsState.SelectAll;
    public bool IsAllPupilsDeselected => _state == SelectAllPupilsState.DeselectAll;
    public bool IsAnyPupilSelected => IsAllPupilsSelected || _pupilsToSelectedMap.Values.Any(t => t);
    public IEnumerable<string> CurrentPageOfPupils { get; set; }

    public static MyPupilsPupilSelectionState CreateDefault() => new();

    public bool IsPupilSelected(string upn)
    {
        if (IsAllPupilsSelected)
        {
            return true;
        }

        if (_pupilsToSelectedMap.TryGetValue(upn, out bool selected))
        {
            return selected;
        }

        return false;
    }

    public IReadOnlyDictionary<string, bool> GetPupilsWithSelectionState() => _pupilsToSelectedMap.AsReadOnly();

    public void SelectAllPupils()
    {
        _state = SelectAllPupilsState.SelectAll;

        if (_pupilsToSelectedMap.Keys.Count == 0)
        {
            return;
        }

        UpsertPupilWithSelectedState(
            _pupilsToSelectedMap.Keys,
            isSelected: true);
    }

    public void DeselectAllPupils()
    {
        _state = SelectAllPupilsState.DeselectAll;

        if (_pupilsToSelectedMap.Keys.Count == 0)
        {
            return;
        }

        UpsertPupilWithSelectedState(
            _pupilsToSelectedMap.Keys,
            isSelected: false);
    }

    public void UpsertPupilWithSelectedState(IEnumerable<string> upns, bool isSelected)
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

            _pupilsToSelectedMap[upn] = isSelected;
        }
    }

    public void ResetState()
    {
        _pupilsToSelectedMap.Clear();
        _state = SelectAllPupilsState.NotSpecified;
    }

    private enum SelectAllPupilsState
    {
        SelectAll,
        DeselectAll,
        NotSpecified
    }

}
