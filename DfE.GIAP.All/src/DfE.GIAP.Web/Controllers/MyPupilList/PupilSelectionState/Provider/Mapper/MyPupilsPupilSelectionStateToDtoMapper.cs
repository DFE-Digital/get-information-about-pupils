using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Web.Controllers.MyPupilList.PupilSelectionState.Provider.DataTransferObjects;

namespace DfE.GIAP.Web.Controllers.MyPupilList.PupilSelectionState.Provider.Mapper;

public sealed class MyPupilsPupilSelectionStateToDtoMapper : IMapper<IMyPupilsPupilSelectionState, MyPupilsPupilSelectionStateDto>
{
    public MyPupilsPupilSelectionStateDto Map(IMyPupilsPupilSelectionState source)
    {
        PupilSelectionModeDto selectionState =
            source.IsAllPupilsSelected ? PupilSelectionModeDto.SelectAll :
            source.IsAllPupilsDeselected ? PupilSelectionModeDto.DeselectAll : PupilSelectionModeDto.NotSpecified;

        return new MyPupilsPupilSelectionStateDto
        {

            PupilUpnToSelectedMap = new Dictionary<string, bool>(source.GetPupilsWithSelectionState()),
            State = selectionState
        };
    }
}
