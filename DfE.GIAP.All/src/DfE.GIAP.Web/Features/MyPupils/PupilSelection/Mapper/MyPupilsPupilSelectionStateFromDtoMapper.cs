using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection.Mapper.DataTransferObjects;

namespace DfE.GIAP.Web.Features.MyPupils.SelectionState.Mapper;
public sealed class MyPupilsPupilSelectionStateFromDtoMapper
    : IMapper<MyPupilsPupilSelectionStateDto, MyPupilsPupilSelectionState>
{
    public MyPupilsPupilSelectionState Map(MyPupilsPupilSelectionStateDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        MyPupilsPupilSelectionState state = MyPupilsPupilSelectionState.CreateDefault();

        SelectionMode mode = dto.Mode;

        switch (mode)
        {
            // Select All Mode: All selected EXCEPT explicit deselections
            case SelectionMode.All:

                state.SelectAll();
                foreach (string upn in dto.DeselectedExceptions?.Where(t => !string.IsNullOrWhiteSpace(t)) ?? [])
                {
                    state.Deselect(upn);
                }

                break;

            // Manual mode: Explicit selections only.
            case SelectionMode.Manual:
            default:
                foreach (string upn in dto.ExplicitSelections?.Where(t => !string.IsNullOrWhiteSpace(t)) ?? [])
                {
                    state.Select(upn);
                }

                break;
        }

        return state;
    }
}
