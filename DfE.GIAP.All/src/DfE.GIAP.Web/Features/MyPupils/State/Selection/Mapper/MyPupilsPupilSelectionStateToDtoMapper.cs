using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Web.Features.MyPupils.State.Selection.DataTransferObjects;

namespace DfE.GIAP.Web.Features.MyPupils.State.Selection.Mapper;

public sealed class MyPupilsPupilSelectionStateToDtoMapper : IMapper<MyPupilsPupilSelectionState, MyPupilsPupilSelectionStateDto>
{
    public MyPupilsPupilSelectionStateDto Map(MyPupilsPupilSelectionState source)
    {
        ArgumentNullException.ThrowIfNull(source);
#pragma warning disable S3358 // Ternary operators should not be nested
        return new MyPupilsPupilSelectionStateDto
        {
            PupilUpnToSelectedMap =
                source.GetPupilsWithSelectionState().ToDictionary((kv) => kv.Key.Value, v => v.Value),
            State =
                source.IsAllPupilsSelected ?
                    PupilSelectionModeDto.SelectAll :
                    source.IsAllPupilsDeselected ?
                        PupilSelectionModeDto.DeselectAll : PupilSelectionModeDto.ManualSelection
        };
#pragma warning restore S3358 // Ternary operators should not be nested
    }
}
