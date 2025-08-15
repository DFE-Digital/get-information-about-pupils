using DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PupilSelectionState.Dto;

namespace DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PupilSelectionState;

public sealed class PupilsSelectionState
{
    private readonly Dictionary<string, bool> _pupilsToSelectedMap = [];
    private SelectAllPupilsState _state;
    public PupilsSelectionState()
    {
        Clear();
    }

    public bool IsAllPupilsSelected => _state == SelectAllPupilsState.SelectAll;

    public IEnumerable<string> SelectedPupils => _pupilsToSelectedMap.Where(t => t.Value).Select(t => t.Key);

    public void AddPupils(IEnumerable<string> upns)
    {
        upns?.ToList().ForEach((upn) => _pupilsToSelectedMap[upn] = false);
    }

    public bool IsPupilSelected(string upn)
    {
        if (!_pupilsToSelectedMap.TryGetValue(upn, out bool selected) || !selected)
        {
            return false;
        }
        return true;
    }

    public void SelectAll()
    {
        _state = SelectAllPupilsState.SelectAll;
        foreach (string item in _pupilsToSelectedMap.Keys)
        {
            _pupilsToSelectedMap[item] = true;
        }
    }

    public void DeselectAll()
    {
        _state = SelectAllPupilsState.DeselectAll;
        foreach (string item in _pupilsToSelectedMap.Keys)
        {
            _pupilsToSelectedMap[item] = false;
        }
    }

    public void MarkSelected(string upn)
    {
        _pupilsToSelectedMap[upn] = true;
    }

    public void MarkDeselected(string upn)
    {
        _pupilsToSelectedMap[upn] = false;
    }

    public void Clear()
    {
        _pupilsToSelectedMap.Clear();
        _state = SelectAllPupilsState.NotSpecified;
    }

    public PupilSelectionStateDto ToDto() => new()
    {
        PupilUpnToSelectedMap = new(_pupilsToSelectedMap),
        State = _state
    };

    public static PupilsSelectionState FromDto(PupilSelectionStateDto dto)
    {
        PupilsSelectionState state = new()
        {
            _state = dto.State
        };

        foreach (KeyValuePair<string, bool> kvp in dto.PupilUpnToSelectedMap)
        {
            state._pupilsToSelectedMap[kvp.Key] = kvp.Value;
        }
        return state;
    }
}
