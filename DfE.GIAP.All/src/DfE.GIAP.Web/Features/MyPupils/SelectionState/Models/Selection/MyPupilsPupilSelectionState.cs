namespace DfE.GIAP.Web.Features.MyPupils.State.Models.Selection;

// Note: SelectAll or DeselectAll states - may have a subsequent manual selection/deselection applied, so this state is applied at the point of SelectAll/Deselect", not infer all future SelectionState from it. i.e. a pupil can be manually deselected after SelectAll has been applied
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

    // TODO can we create a GetSelectedPupils - which switches on mode

    public IReadOnlyCollection<string> GetExplicitSelections()
        => _explicitSelections.ToList().AsReadOnly();

    public IReadOnlyCollection<string> GetDeselectedExceptions()
        => _deselectionUpnExceptions.ToList().AsReadOnly();
}

public enum SelectionMode
{
    None, // Track explicit selections only.
    All   // Everything selected by default; track deselection exceptions.
}
