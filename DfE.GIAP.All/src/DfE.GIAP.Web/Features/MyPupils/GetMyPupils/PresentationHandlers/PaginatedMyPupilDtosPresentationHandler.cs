using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.Web.Features.MyPupils.State.Presentation;

namespace DfE.GIAP.Web.Features.MyPupils.GetMyPupils.PresentationHandlers;

public sealed class PaginateMyPupilsModelPresentationHandler : IMyPupilsModelPresentationHandler
{
    public MyPupilsModel Handle(MyPupilsModel myPupils, MyPupilsPresentationState state)
    {
        const int DefaultPageSize = 20;

        PageNumber page = PageNumber.Page(state.Page);

        int skip = DefaultPageSize * (page.Value - 1);

        if (skip >= myPupils.Count)
        {
            return MyPupilsModel.Create([]);
        }

        List<MyPupilModel> pagedResults = myPupils.Values
                .Skip(skip)
                .Take(DefaultPageSize)
                .ToList();

        return MyPupilsModel.Create(pupils: pagedResults);
    }
}
