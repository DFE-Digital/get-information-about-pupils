using DfE.GIAP.Core.Common.CrossCutting;

namespace DfE.GIAP.Web.Controllers.MyPupilList.PupilSelectionState;

public sealed class MyPupilsPupilSelectionState : IMyPupilsPupilSelectionState
{
    private readonly IDictionary<string, bool> _pupilsToSelectedMap;
    private SelectAllPupilsState _state = SelectAllPupilsState.NotSpecified;

    public MyPupilsPupilSelectionState()
    {
        _pupilsToSelectedMap = new Dictionary<string, bool>();
    }

    public MyPupilsPupilSelectionState(IDictionary<string, bool> pupilsToSelectedMap)
    {
        ArgumentNullException.ThrowIfNull(pupilsToSelectedMap);
        _pupilsToSelectedMap = pupilsToSelectedMap;
    }

    public bool IsAllPupilsSelected => _state == SelectAllPupilsState.SelectAll;
    public bool IsAllPupilsDeselected => _state == SelectAllPupilsState.DeselectAll;
    public bool IsAnyPupilsSelected => IsAllPupilsSelected || GetSelectedPupils().Count != 0;
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

    public IReadOnlyCollection<string> GetSelectedPupils()
    {
        return _pupilsToSelectedMap
            .Where(t => t.Value)
            .Select(t => t.Key)
            .ToList()
            .AsReadOnly();
    }

    public void AddPupils(IEnumerable<string> upns) => UpdatePupilSelectionState(upns, isSelected: false);

    public void SelectAllPupils()
    {
        _state = SelectAllPupilsState.SelectAll;

        if (_pupilsToSelectedMap.Keys.Count == 0)
        {
            return;
        }

        UpdatePupilSelectionState(_pupilsToSelectedMap.Keys, isSelected: true);
    }

    public void DeselectAllPupils()
    {
        _state = SelectAllPupilsState.DeselectAll;

        if (_pupilsToSelectedMap.Keys.Count == 0)
        {
            return;
        }
        UpdatePupilSelectionState(_pupilsToSelectedMap.Keys, isSelected: false);
    }

    public void UpdatePupilSelectionState(IEnumerable<string> upns, bool isSelected)
    {
        ArgumentNullException.ThrowIfNull(upns);

        foreach (string upn in upns)
        {
            if (!UniquePupilNumberValidator.Validate(upn))
            {
                throw new ArgumentException("Invalid UPN requested");
            }

            _pupilsToSelectedMap[upn] = isSelected;
        }
    }

    public void ClearPupilsAndState()
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
