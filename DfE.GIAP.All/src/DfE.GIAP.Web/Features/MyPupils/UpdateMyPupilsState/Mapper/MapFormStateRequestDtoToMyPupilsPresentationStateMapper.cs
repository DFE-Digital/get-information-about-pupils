using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Web.Features.MyPupils.State.Presentation;

namespace DfE.GIAP.Web.Features.MyPupils.UpdateMyPupilsState.Mapper;

internal sealed class MapFormStateRequestDtoToMyPupilsPresentationStateMapper : IMapper<MyPupilsFormStateRequestDto, MyPupilsPresentationState>
{
    public MyPupilsPresentationState Map(MyPupilsFormStateRequestDto input)
    {
        return new(
            input.PageNumber,
            input.SortField,
            input.SortDirection == "asc" ? SortDirection.Ascending : SortDirection.Descending);
    }
}
