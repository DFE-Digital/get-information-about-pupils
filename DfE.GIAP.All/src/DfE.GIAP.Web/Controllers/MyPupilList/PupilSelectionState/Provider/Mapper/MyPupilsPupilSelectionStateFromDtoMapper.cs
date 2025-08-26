using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Web.Controllers.MyPupilList.PupilSelectionState.Provider.DataTransferObjects;

namespace DfE.GIAP.Web.Controllers.MyPupilList.PupilSelectionState.Provider.Mapper;

public sealed class MyPupilsPupilSelectionStateFromDtoMapper : IMapper<MyPupilsPupilSelectionStateDto, IMyPupilsPupilSelectionState>
{
    public IMyPupilsPupilSelectionState Map(MyPupilsPupilSelectionStateDto input)
    {
        // Construct with initial selection map
        MyPupilsPupilSelectionState state = new(input.PupilUpnToSelectedMap);

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
                // No action needed; already initialized
                break;
        }

        return state;

    }
}
