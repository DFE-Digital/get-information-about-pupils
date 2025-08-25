using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Web.Controllers.MyPupilList.PupilSelectionState.Provider.DataTransferObjects;
using Mono.TextTemplating;

namespace DfE.GIAP.Web.Controllers.MyPupilList.PupilSelectionState;

public sealed class PupilsSelectionState : IPupilsSelectionState
{
    private readonly Dictionary<string, bool> _pupilsToSelectedMap = [];
    private SelectAllPupilsState _state = SelectAllPupilsState.NotSpecified;

    public bool IsAllPupilsSelected => _state == SelectAllPupilsState.SelectAll;
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

    public void ResetState()
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
