using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Web.Features.MyPupils.SelectionState;
using DfE.GIAP.Web.Features.MyPupils.SelectionState.Provider.DataTransferObjects;

namespace DfE.GIAP.Web.Features.MyPupils.SelectionState.Provider.Mapper;

public class MyPupilsPupilSelectionStateFromDtoMapper : IMapper<MyPupilsPupilSelectionStateDto, MyPupilsPupilSelectionState>
{
    public MyPupilsPupilSelectionState Map(MyPupilsPupilSelectionStateDto input)
    {
        // Construct with initial selection map
        MyPupilsPupilSelectionState state = new();

        // Apply selection mode
        switch (input.State)
        {
            case PupilSelectionModeDto.SelectAll:
                state.SelectAllPupils();
                break;
            case PupilSelectionModeDto.DeselectAll:
                state.DeselectAllPupils();
                break;
            case PupilSelectionModeDto.NotSpecified:
            default:
                IEnumerable<string> selectedPupils = state.GetPupilsWithSelectionState().Where(t => t.Value).Select(t => t.Key);
                state.UpsertPupilWithSelectedState(selectedPupils, isSelected: true);

                IEnumerable<string> notSelectedPupils = state.GetPupilsWithSelectionState().Where(t => !t.Value).Select(t => t.Key);
                state.UpsertPupilWithSelectedState(notSelectedPupils, isSelected: false);
                break;
        }

        return state;
    }
}
