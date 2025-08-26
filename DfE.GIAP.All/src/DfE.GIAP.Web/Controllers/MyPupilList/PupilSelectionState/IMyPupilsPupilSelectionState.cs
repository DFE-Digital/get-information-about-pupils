namespace DfE.GIAP.Web.Controllers.MyPupilList.PupilSelectionState;

public interface IMyPupilsPupilSelectionState
{
    bool IsAllPupilsSelected { get; }
    bool IsAnyPupilsSelected { get; }
    bool IsAllPupilsDeselected { get; }
    bool IsPupilSelected(string upn);
    IReadOnlyDictionary<string, bool> GetPupilsWithSelectionState();
    IReadOnlyCollection<string> GetSelectedPupils();
    void AddPupils(IEnumerable<string> upns);
    void SelectAllPupils();
    void DeselectAllPupils();
    void UpdatePupilSelectionState(IEnumerable<string> upns, bool isSelected);
    void ClearPupilsAndState();
}
