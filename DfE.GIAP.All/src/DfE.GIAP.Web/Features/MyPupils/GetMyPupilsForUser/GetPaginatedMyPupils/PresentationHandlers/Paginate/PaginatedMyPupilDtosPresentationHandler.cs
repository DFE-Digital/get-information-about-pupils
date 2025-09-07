using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Web.Features.MyPupils.GetMyPupilsForUser.GetPaginatedMyPupils.PresentationHandlers;
using DfE.GIAP.Web.Features.MyPupils.State.Presentation;

namespace DfE.GIAP.Web.Features.MyPupils.GetMyPupilsForUser.GetPaginatedMyPupils.PresentationHandlers.Paginate;

public sealed class PaginateMyPupilDtosPresentationHandler : IMyPupilDtosPresentationHandler
{
    public MyPupilDtos Handle(MyPupilDtos pupils, MyPupilsPresentationState state)
    {
        const int DefaultPageSize = 20;

        PageNumber page = PageNumber.Page(state.Page);

        int skip = DefaultPageSize * (page.Value - 1);

        if (skip >= pupils.Count)
        {
            return MyPupilDtos.Create([]);
        }

        List<MyPupilDto> pagedResults = pupils.Values
                .Skip(skip)
                .Take(DefaultPageSize)
                .ToList();

        return MyPupilDtos.Create(pupils: pagedResults);
    }
}
