using DfE.GIAP.Web.Controllers.MyPupilList.PupilSelectionState.Provider.DataTransferObjects;

namespace DfE.GIAP.Web.Controllers.MyPupilList.PupilSelectionState;

public interface IPupilsSelectionState
{
    bool IsAllPupilsSelected { get; }
    bool IsAnyPupilsSelected { get; }
    bool IsPupilSelected(string upn);
    IReadOnlyCollection<string> GetSelectedPupils();
    void AddPupils(IEnumerable<string> upns);
    void SelectAllPupils();
    void DeselectAllPupils();
    void UpdatePupilSelectionState(IEnumerable<string> upns, bool isSelected);
    void ResetState();
    PupilSelectionStateDto ToDto();
}
