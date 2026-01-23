using DfE.GIAP.Web.Features.MyPupils.PresentationService.GetPupils;
using DfE.GIAP.Web.Features.MyPupils.PresentationService.Models;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection.Operations;

#nullable enable
namespace DfE.GIAP.Web.Features.MyPupils.PresentationService.PresentationHandlers;

public record MyPupilsPresentationHandlerRequest
{
    public MyPupilsPresentationHandlerRequest(
        MyPupilsPresentationPupilModels? pupils,
        MyPupilsPresentationQueryModel? query,
        MyPupilsPupilSelectionState? selectionState)
    {
        Pupils = pupils ?? MyPupilsPresentationPupilModels.Create([]);

        Query = query ?? MyPupilsPresentationQueryModel.CreateDefault();

        SelectionState = selectionState ?? MyPupilsPupilSelectionState.CreateDefault();
    }

    public MyPupilsPresentationPupilModels Pupils { get; }
    public MyPupilsPresentationQueryModel Query { get; }
    public MyPupilsPupilSelectionState SelectionState { get; }
}
