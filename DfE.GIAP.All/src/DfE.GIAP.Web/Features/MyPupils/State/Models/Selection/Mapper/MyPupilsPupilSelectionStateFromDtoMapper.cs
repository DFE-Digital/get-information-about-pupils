using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Web.Features.MyPupils.State.Models.Selection.DataTransferObjects;

namespace DfE.GIAP.Web.Features.MyPupils.State.Models.Selection.Mapper;
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
                if (dto.DeselectionExceptions is { Count: > 0 })
                {
                    foreach (string upn in dto.DeselectionExceptions)
                    {
                        if (!string.IsNullOrWhiteSpace(upn))
                        {
                            state.Deselect(upn);
                        }
                    }
                }
                break;

            case SelectionMode.None:
            default:
                // None mode: apply explicit selections only.
                if (dto.ExplicitSelections is { Count: > 0 })
                {
                    foreach (string upn in dto.ExplicitSelections)
                    {
                        if (!string.IsNullOrWhiteSpace(upn))
                        {
                            state.Select(upn);
                        }
                    }
                }
                break;
        }

        return state;
    }
}
