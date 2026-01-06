using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Web.Features.MyPupils.SelectionState.DataTransferObjects;

namespace DfE.GIAP.Web.Features.MyPupils.SelectionState.Mapper;
public sealed class MyPupilsPupilSelectionStateFromDtoMapper
    : IMapper<MyPupilsPupilSelectionStateDto, MyPupilsPupilSelectionState>
{
    public MyPupilsPupilSelectionState Map(MyPupilsPupilSelectionStateDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        MyPupilsPupilSelectionState state = new();

        SelectionMode mode = dto.Mode;

        switch (mode)
        {
            case SelectionMode.All:

                state.SelectAll();

                foreach (string upn in dto.DeselectionExceptions?.Where(t => !string.IsNullOrWhiteSpace(t)) ?? [])
                {
                    state.Deselect(upn);
                }

                break;

            case SelectionMode.Manual:
            default:

                // None mode: apply explicit selections only.
                foreach (string upn in dto.ExplicitSelections?.Where(t => !string.IsNullOrWhiteSpace(t)) ?? [])
                {
                    state.Select(upn);
                }

                break;
        }

        return state;
    }
}
