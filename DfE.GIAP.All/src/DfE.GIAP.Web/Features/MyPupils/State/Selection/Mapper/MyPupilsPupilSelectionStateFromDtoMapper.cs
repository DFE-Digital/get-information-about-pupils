using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.Extensions;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
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
                state.UpsertPupilSelectionState(input.PupilUpnToSelectedMap.Keys.ToUniquePupilNumbers(), false);
                state.SelectAllPupils();
                break;
            case PupilSelectionModeDto.DeselectAll:
                state.UpsertPupilSelectionState(input.PupilUpnToSelectedMap.Keys.ToUniquePupilNumbers(), true);
                state.DeselectAllPupils();
                break;
            default:
                IEnumerable<UniquePupilNumber> selectedPupils = input.PupilUpnToSelectedMap.Where(t => t.Value).Select(t => t.Key).ToUniquePupilNumbers();
                state.UpsertPupilSelectionState(selectedPupils, isSelected: true);

                IEnumerable<UniquePupilNumber> notSelectedPupils = input.PupilUpnToSelectedMap.Where(t => !t.Value).Select(t => t.Key).ToUniquePupilNumbers();
                state.UpsertPupilSelectionState(notSelectedPupils, isSelected: false);
                break;
        }

        return state;
    }
}
