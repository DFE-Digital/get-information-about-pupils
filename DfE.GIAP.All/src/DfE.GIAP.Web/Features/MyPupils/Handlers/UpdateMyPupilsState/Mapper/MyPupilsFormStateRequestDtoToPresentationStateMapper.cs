using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Web.Features.MyPupils;
using DfE.GIAP.Web.Features.MyPupils.PresentationState;

namespace DfE.GIAP.Web.Features.MyPupils.Handlers.UpdateMyPupilsState.Mapper;

public sealed class MyPupilsFormStateRequestDtoToPresentationStateMapper : IMapper<MyPupilsFormStateRequestDto, MyPupilsPresentationState>
{
    public MyPupilsPresentationState Map(MyPupilsFormStateRequestDto input)
    {
        return new(
            input.PageNumber,
            input.SortField,
            input.SortDirection == "asc" ? SortDirection.Ascending : SortDirection.Descending);
    }
}
