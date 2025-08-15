using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PresenterHandler.Order;

namespace DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PresenterHandler.Options;

public sealed class MapMyPupilsFormStateRequestDtoToPresentationOptions : IMapper<MyPupilsFormStateRequestDto, PresentPupilsOptions>
{
    public PresentPupilsOptions Map(MyPupilsFormStateRequestDto input)
    {
        return new(
            input.PageNumber,
            input.SortField,
            input.SortDirection == "asc" ? SortDirection.Ascending : SortDirection.Descending);
    }
}
