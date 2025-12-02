using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Web.Features.MyPupils.State.Selection.DataTransferObjects;

namespace DfE.GIAP.Web.Features.MyPupils.State.Selection.Mapper;

public class MyPupilsPupilSelectionStateFromDtoMapper : IMapper<MyPupilsPupilSelectionStateDto, MyPupilsPupilSelectionState>
{
    public MyPupilsPupilSelectionState Map(MyPupilsPupilSelectionStateDto input)
    {
        ArgumentNullException.ThrowIfNull(input);

        // Construct with initial selection map
        MyPupilsPupilSelectionState state = new();

        // Apply selection mode
        switch (input.State)
        {
            case PupilSelectionModeDto.SelectAll:
                state.UpsertPupilSelectionState(input.PupilUpnToSelectedMap.Keys, false);
                state.SelectAllPupils();
                break;
            case PupilSelectionModeDto.DeselectAll:
                state.UpsertPupilSelectionState(input.PupilUpnToSelectedMap.Keys, true);
                state.DeselectAllPupils();
                break;
            default:
                IEnumerable<string> selectedPupils = input.PupilUpnToSelectedMap.Select(t => t.Key);
                state.UpsertPupilSelectionState(selectedPupils, isSelected: true);

                IEnumerable<string> notSelectedPupils = input.PupilUpnToSelectedMap.Select(t => t.Key);
                state.UpsertPupilSelectionState(notSelectedPupils, isSelected: false);
                break;
        }

        return state;
    }
}
