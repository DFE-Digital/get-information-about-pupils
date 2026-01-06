using DfE.GIAP.Web.Features.MyPupils.PresentationService.Models;
using DfE.GIAP.Web.Features.MyPupils.SelectionState;

namespace DfE.GIAP.Web.Features.MyPupils.PresentationService.PresentationHandlers;

public sealed class PaginateMyPupilsModelPresentationHandler : IMyPupilsPresentationModelHandler
{
    public MyPupilsPresentationPupilModels Handle(
        MyPupilsPresentationPupilModels myPupils,
        MyPupilsPresentationQueryModel query,
        MyPupilsPupilSelectionState selectionState)
    {
        const int DefaultPageSize = 20;

        int skip = DefaultPageSize * (query.Page.Value - 1);

        if (skip >= myPupils.Count)
        {
            return MyPupilsPresentationPupilModels.Empty();
        }

        List<MyPupilsPresentationPupilModel> pagedResults = myPupils.Values
            .Skip(skip)
            .Take(DefaultPageSize)
            .ToList();

        return MyPupilsPresentationPupilModels.Create(pagedResults);
    }
}
