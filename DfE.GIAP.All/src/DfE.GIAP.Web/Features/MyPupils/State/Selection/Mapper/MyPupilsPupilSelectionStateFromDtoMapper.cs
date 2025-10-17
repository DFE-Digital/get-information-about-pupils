using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.Extensions;
using DfE.GIAP.Web.Features.MyPupils.State.Selection.DataTransferObjects;

namespace DfE.GIAP.Web.Features.MyPupils.State.Selection.Mapper;

public class MyPupilsPupilSelectionStateFromDtoMapper : IMapper<MyPupilsPupilSelectionStateDto, MyPupilsPupilSelectionState>
{
    public MyPupilsPupilSelectionState Map(MyPupilsPupilSelectionStateDto input)
    {
        ArgumentNullException.ThrowIfNull(input);

        // Construct with initial selection map
        MyPupilsPupilSelectionState state = new();

        // Apply SelectionMode
        if(input.State == PupilSelectionModeDto.SelectAll)
        {
            state.SelectAllPupils();
        }

        else if(input.State == PupilSelectionModeDto.DeselectAll)
        {
            state.DeselectAllPupils();
        }

        // Then apply selectedState of each pupil
        List<string> selectedPupils = input.PupilUpnToSelectedMap.Where(t => t.Value).Select(t => t.Key).ToList();
        state.UpsertPupilSelectionState(selectedPupils, isSelected: true);

        List<string> deselectedPupils = input.PupilUpnToSelectedMap.Where(t => !t.Value).Select(t => t.Key).ToList();
        state.UpsertPupilSelectionState(deselectedPupils, isSelected: false);

        return state;
    }
}
