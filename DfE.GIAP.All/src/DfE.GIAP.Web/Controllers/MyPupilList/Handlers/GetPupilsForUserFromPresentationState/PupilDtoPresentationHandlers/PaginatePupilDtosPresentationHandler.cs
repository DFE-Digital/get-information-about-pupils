using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Web.Controllers.MyPupilList.PresentationState;

namespace DfE.GIAP.Web.Controllers.MyPupilList.Handlers.GetPupilsForUserFromPresentationState.PupilDtoPresentationHandlers;

public sealed class PaginatePupilDtosPresentationHandler : IPupilDtoPresentationHandler
{
    public IEnumerable<PupilDto> Handle(IEnumerable<PupilDto> pupils, MyPupilsPresentationState options)
    {
        const int DefaultPageSize = 20;
        PageNumber page = PageNumber.Page(options.Page);
        int skip = DefaultPageSize * (page.Value - 1);

        if (skip >= pupils.Count())
        {
            return [];
        }

        List<PupilDto> pagedResults = pupils
            .Skip(skip)
            .Take(DefaultPageSize)
            .ToList();

        return pagedResults;
    }
}
