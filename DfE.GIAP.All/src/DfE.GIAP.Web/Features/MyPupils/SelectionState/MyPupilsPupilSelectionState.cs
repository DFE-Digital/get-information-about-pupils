namespace DfE.GIAP.Web.Features.MyPupils.SelectionState;

public sealed class MyPupilsPupilSelectionState
{
    private SelectionMode _mode = SelectionMode.None;

    // In None mode, we keep explicit selections.
    private readonly HashSet<string> _explicitSelections = [];

    // In All mode, we keep deselected UPNs.
    private readonly HashSet<string> _deselectedUpnExceptions = [];

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
        _deselectedUpnExceptions.Clear();
    }

    public void SelectAll()
    {
        _mode = SelectionMode.All;
        _explicitSelections.Clear();
        _deselectedUpnExceptions.Clear();
    }

    public void DeselectAll()
    {
        _mode = SelectionMode.None;
        _explicitSelections.Clear();
        _deselectedUpnExceptions.Clear();
    }

    public void Select(string upn)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(upn);

        if (_mode == SelectionMode.All)
        {
            _deselectedUpnExceptions.Remove(upn); // selecting = remove exception
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
            _deselectedUpnExceptions.Add(upn); // deselecting = add exception
        }
        else
        {
            _explicitSelections.Remove(upn);
        }
    }

    public bool IsPupilSelected(string upn)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(upn);

        return _mode == SelectionMode.All
            ? !_deselectedUpnExceptions.Contains(upn)
            : _explicitSelections.Contains(upn);
    }

    public IReadOnlyCollection<string> GetExplicitSelections()
        => _explicitSelections.ToList().AsReadOnly();

    public IReadOnlyCollection<string> GetDeselectedExceptions()
        => _deselectedUpnExceptions.ToList().AsReadOnly();
}

// Note: SelectAll state - may have a subsequent manual selection/deselection applied, so "SelectAll" is applied at the point of SelectAll/Deselect" than
// infer ALL selectionStates from this mode.
// i.e. a pupil can be manually deselected after SelectAll has been applied

public enum SelectionMode
{
    None, // Track explicit selections only.
    All   // Everything selected by default; track deselection exceptions.
}
