using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PresenterHandler;
using DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PresenterHandler.Options;

namespace DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PresenterHandler.Paginate;

public sealed class PaginatePupilDtosPresentationHandler : IPupilDtoPresentationHandler
{
    public IEnumerable<PupilDto> Handle(IEnumerable<PupilDto> pupils, PresentPupilsOptions options)
    {
        const int DefaultPageSize = 20;
        PageNumber page = PageNumber.Page(options.Page);
        int skip = DefaultPageSize * (page.Value - 1);

        List<PupilDto> pagedResults = pupils
            .Skip(skip)
            .Take(DefaultPageSize)
            .ToList();

        return pagedResults;
    }
}
