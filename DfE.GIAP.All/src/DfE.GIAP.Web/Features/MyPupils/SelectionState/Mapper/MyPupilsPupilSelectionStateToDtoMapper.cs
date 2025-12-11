using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Web.Features.MyPupils.SelectionState.DataTransferObjects;

namespace DfE.GIAP.Web.Features.MyPupils.SelectionState.Mapper;

public sealed class MyPupilsPupilSelectionStateToDtoMapper : IMapper<MyPupilsPupilSelectionState, MyPupilsPupilSelectionStateDto>
{
    public MyPupilsPupilSelectionStateDto Map(MyPupilsPupilSelectionState source)
    {
#pragma warning disable S3358 // Ternary operators should not be nested

        ArgumentNullException.ThrowIfNull(source);

        if (source.Mode == SelectionMode.All)
        {
            return new MyPupilsPupilSelectionStateDto
            {
                Mode = SelectionMode.All,
                // In All mode, we only persist the exceptions.
                // ExplicitSelections should be empty by design.
                ExplicitSelections = [],
                DeselectionExceptions = [.. source.GetDeselectedExceptions()]
            };
        }

        // None mode: persist explicit selections only.
        return new MyPupilsPupilSelectionStateDto
        {
            Mode = SelectionMode.None,
            ExplicitSelections = [.. source.GetExplicitSelections()],
            DeselectionExceptions = []
        };

#pragma warning restore S3358 // Ternary operators should not be nested
    }
}
