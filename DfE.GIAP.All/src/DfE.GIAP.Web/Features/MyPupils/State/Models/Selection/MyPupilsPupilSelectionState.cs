namespace DfE.GIAP.Web.Features.MyPupils.State.Models.Selection;

// Note: SelectAll or DeselectAll states - may have a subsequent manual selection/deselection applied, so this state is applied at the point of SelectAll/Deselect", not infer all future SelectionState from it. i.e. a pupil can be manually deselected after SelectAll has been applied


public enum SelectionMode
{
    None, // Track explicit selections only.
    All   // Everything selected by default; track deselection exceptions.
}

public sealed class MyPupilsPupilSelectionState
{
    private SelectionMode _mode = SelectionMode.None;

    // In None mode, we keep explicit selections.
    private readonly HashSet<string> _explicitSelections = [];

    // In All mode, we keep exceptions (deselected UPNs).
    private readonly HashSet<string> _deselectionUpnExceptions = [];

    public MyPupilsPupilSelectionState()
    {
        ResetState();
    }

    public static MyPupilsPupilSelectionState CreateDefault() => new();

    public SelectionMode Mode => _mode;

    public bool IsAnyPupilSelected => _mode == SelectionMode.All || _explicitSelections.Count > 0;

    public void ResetState()
    {
        _mode = SelectionMode.None;
        _explicitSelections.Clear();
        _deselectionUpnExceptions.Clear();
    }

    public void SelectAll()
    {
        _mode = SelectionMode.All;
        _explicitSelections.Clear();
        _deselectionUpnExceptions.Clear();
    }

    public void DeselectAll()
    {
        _mode = SelectionMode.None;
        _explicitSelections.Clear();
        _deselectionUpnExceptions.Clear();
    }

    public void Select(string upn)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(upn);

        if (_mode == SelectionMode.All)
        {
            _deselectionUpnExceptions.Remove(upn); // selecting = remove exception
        }
        else
        {
            _explicitSelections.Add(upn);
        }
    }

    public void Deselect(string upn)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(upn);

        if (_mode == SelectionMode.All)
        {
            _deselectionUpnExceptions.Add(upn); // deselecting = add exception
        }
        else
        {
            _explicitSelections.Remove(upn);
        }
    }

    public void UpsertPupilSelections(IEnumerable<string> upns, bool isSelected)
    {
        ArgumentNullException.ThrowIfNull(upns);
        foreach (string upn in upns)
        {
            if (isSelected)
            {
                Select(upn);
                continue;
            }

            Deselect(upn);
        }
    }

    public bool IsPupilSelected(string upn)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(upn);

        return _mode == SelectionMode.All
            ? !_deselectionUpnExceptions.Contains(upn)
            : _explicitSelections.Contains(upn);
    }

    public IReadOnlyCollection<string> GetExplicitSelections() => _explicitSelections.ToList().AsReadOnly();
    public IReadOnlyCollection<string> GetDeselectedExceptions() => _deselectionUpnExceptions.ToList().AsReadOnly();
}


/*public sealed class MyPupilsPupilSelectionState
{
    public MyPupilsPupilSelectionState()
    {
        ResetState();
    }

    private readonly Dictionary<string, bool> _pupilsToSelectedMap = [];

    *//*public bool IsAllPupilsSelected => _state == SelectionState.SelectAll;
*//*
    public bool IsAnyPupilSelected => _pupilsToSelectedMap.Values.Any(t => t);

    public static MyPupilsPupilSelectionState CreateDefault() => new();

    public IReadOnlyDictionary<string, bool> GetPupilsWithSelectionState() => _pupilsToSelectedMap.AsReadOnly();
    public IReadOnlyList<string> GetSelectedPupils()
        => _pupilsToSelectedMap
                .Where((pupilUpn) => pupilUpn.Value)
                .Select(t => t.Key)
                .ToList().AsReadOnly();

    public bool IsPupilSelected(string upn) => _pupilsToSelectedMap.TryGetValue(upn, out bool selected) && selected;

    public void SelectAllPupils()
    {
        UpsertPupilSelections(
            _pupilsToSelectedMap.Keys,
            isSelected: true);
    }

    public void DeselectAllPupils()
    {
        UpsertPupilSelections(
            _pupilsToSelectedMap.Keys,
            isSelected: false);
    }

    public void RemovePupils(IEnumerable<string> upns)
    {
        upns.ToList().ForEach((upn) =>
        {
            _pupilsToSelectedMap.Remove(upn);
        });
    }

    public void ResetState()
    {
        _pupilsToSelectedMap.Clear();
    }

    public void UpsertPupilSelections(IEnumerable<string> upns, bool isSelected)
    {
        ArgumentNullException.ThrowIfNull(upns);

        foreach (string upn in upns)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(upn); // TODO UpnValidator?
            _pupilsToSelectedMap[upn] = isSelected;
        }
    }
}*/
