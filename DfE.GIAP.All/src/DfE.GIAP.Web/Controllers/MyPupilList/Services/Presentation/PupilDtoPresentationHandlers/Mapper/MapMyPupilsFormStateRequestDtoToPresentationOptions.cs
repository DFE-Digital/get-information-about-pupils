using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PresenterHandler.Options;
using DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PresenterHandler.Order;

namespace DfE.GIAP.Web.Controllers.MyPupilList.Services.Presentation.PupilDtoPresentationHandlers.Mapper;

public sealed class MapMyPupilsFormStateRequestDtoToPresentationOptions : IMapper<MyPupilsFormStateRequestDto, PupilsPresentationOptions>
{
    public PupilsPresentationOptions Map(MyPupilsFormStateRequestDto input)
    {
        return new(
            input.PageNumber,
            input.SortField,
            input.SortDirection == "asc" ? SortDirection.Ascending : SortDirection.Descending);
    }
}
