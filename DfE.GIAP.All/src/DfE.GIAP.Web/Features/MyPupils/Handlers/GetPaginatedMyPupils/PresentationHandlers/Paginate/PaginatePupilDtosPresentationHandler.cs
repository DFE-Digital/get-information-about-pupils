using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Web.Features.MyPupils.PresentationState;

namespace DfE.GIAP.Web.Features.MyPupils.Handlers.GetPaginatedMyPupils.PresentationHandlers.Paginate;

public sealed class PaginatePupilDtosPresentationHandler : IPupilDtosPresentationHandler
{
    public PupilDtos Handle(PupilDtos pupils, MyPupilsPresentationState state)
    {
        const int DefaultPageSize = 20;

        PageNumber page = PageNumber.Page(state.Page);

        int skip = DefaultPageSize * (page.Value - 1);

        if (skip >= pupils.Count)
        {
            return PupilDtos.Create([]);
        }

        List<PupilDto> pagedResults =
            pupils.Pupils
                .Skip(skip)
                .Take(DefaultPageSize)
                .ToList();

        return PupilDtos.Create(pagedResults);
    }
}
