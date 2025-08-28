using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Web.Features.MyPupils.SelectionState.Provider.DataTransferObjects;

namespace DfE.GIAP.Web.Features.MyPupils.SelectionState.Provider.Mapper;

public sealed class MyPupilsPupilSelectionStateToDtoMapper : IMapper<MyPupilsPupilSelectionState, MyPupilsPupilSelectionStateDto>
{
    public MyPupilsPupilSelectionStateDto Map(MyPupilsPupilSelectionState source)
    {
#pragma warning disable S3358 // Ternary operators should not be nested
        return new MyPupilsPupilSelectionStateDto
        {
            PupilUpnToSelectedMap = source.GetPupilsWithSelectionState().ToDictionary(),
            State =
            source.IsAllPupilsSelected ?
                PupilSelectionModeDto.SelectAll :
                source.IsAllPupilsDeselected ?
                    PupilSelectionModeDto.DeselectAll : PupilSelectionModeDto.NotSpecified
        };
#pragma warning restore S3358 // Ternary operators should not be nested
    }
}
