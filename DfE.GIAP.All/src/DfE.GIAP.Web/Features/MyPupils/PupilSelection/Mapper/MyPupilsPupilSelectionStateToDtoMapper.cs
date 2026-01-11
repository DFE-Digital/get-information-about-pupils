using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection.Mapper.DataTransferObjects;

namespace DfE.GIAP.Web.Features.MyPupils.PupilSelection.Mapper;

public sealed class MyPupilsPupilSelectionStateToDtoMapper : IMapper<MyPupilsPupilSelectionState, MyPupilsPupilSelectionStateDto>
{
    public MyPupilsPupilSelectionStateDto Map(MyPupilsPupilSelectionState source)
    {
#pragma warning disable S3358 // Ternary operators should not be nested

        ArgumentNullException.ThrowIfNull(source);

        // In All mode, we only persist the exceptions.
        // ExplicitSelections should be empty by design.
        if (source.Mode == SelectionMode.All)
        {
            return new MyPupilsPupilSelectionStateDto
            {
                Mode = SelectionMode.All,
                ExplicitSelections = [],
                DeselectedExceptions = [.. source.GetDeselectedExceptions()]
            };
        }

        // Manual mode: persist explicit selections only.
        return new MyPupilsPupilSelectionStateDto
        {
            Mode = SelectionMode.Manual,
            ExplicitSelections = [.. source.GetManualSelections()],
            DeselectedExceptions = []
        };

#pragma warning restore S3358 // Ternary operators should not be nested
    }
}