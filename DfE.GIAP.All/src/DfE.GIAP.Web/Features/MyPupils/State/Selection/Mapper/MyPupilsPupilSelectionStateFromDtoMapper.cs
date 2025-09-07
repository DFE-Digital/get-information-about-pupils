using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Web.Features.MyPupils.State.Selection.DataTransferObjects;

namespace DfE.GIAP.Web.Features.MyPupils.State.Selection.Mapper;

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
            case PupilSelectionModeDto.ManualSelection:
            default:
                IEnumerable<UniquePupilNumber> selectedPupils = state.GetPupilsWithSelectionState().Where(t => t.Value).Select(t => t.Key);
                state.UpsertPupilSelectionState(selectedPupils, isSelected: true);

                IEnumerable<UniquePupilNumber> notSelectedPupils = state.GetPupilsWithSelectionState().Where(t => !t.Value).Select(t => t.Key);
                state.UpsertPupilSelectionState(notSelectedPupils, isSelected: false);
                break;
        }

        return state;
    }
}
