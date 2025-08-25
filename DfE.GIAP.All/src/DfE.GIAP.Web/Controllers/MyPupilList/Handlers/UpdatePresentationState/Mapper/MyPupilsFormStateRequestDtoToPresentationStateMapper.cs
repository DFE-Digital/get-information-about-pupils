using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Web.Controllers.MyPupilList.PresentationState;

namespace DfE.GIAP.Web.Controllers.MyPupilList.Handlers.UpdatePresentationState.Mapper;

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
