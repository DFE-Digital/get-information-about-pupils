using DfE.GIAP.Web.Features.MyPupils.Controllers.UpdateForm;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection.UpdatePupilSelections;
using DfE.GIAP.Web.Features.MyPupils.Services.GetSelectedPupilUpns;

namespace DfE.GIAP.Web.Features.MyPupils.Services.UpsertSelectedPupils;

internal sealed class UpsertSelectedPupilsPresentationService : IUpsertSelectedPupilsIdentifiersPresentationService
{
    private readonly IUpdateMyPupilsPupilSelectionsCommandHandler _updateMyPupilsPupilSelectionsCommandHandler;
    private readonly IGetSelectedPupilsUniquePupilNumbersPresentationService _getSelectedPupilsPresentationHandler;

    public UpsertSelectedPupilsPresentationService(
        IUpdateMyPupilsPupilSelectionsCommandHandler updateMyPupilsPupilSelectionsCommandHandler,
        IGetSelectedPupilsUniquePupilNumbersPresentationService getSelectedPupilsPresentationHandler)
    {
        ArgumentNullException.ThrowIfNull(updateMyPupilsPupilSelectionsCommandHandler);
        _updateMyPupilsPupilSelectionsCommandHandler = updateMyPupilsPupilSelectionsCommandHandler;

        ArgumentNullException.ThrowIfNull(getSelectedPupilsPresentationHandler);
        _getSelectedPupilsPresentationHandler = getSelectedPupilsPresentationHandler;
    }

    public async Task<List<string>> UpsertAsync(string userId, MyPupilsPupilSelectionsRequestDto? selectionsDto)
    {
        if (selectionsDto != null)
        {
            await _updateMyPupilsPupilSelectionsCommandHandler.Handle(selectionsDto);
        }

        List<string> allSelectedPupils =
            (await _getSelectedPupilsPresentationHandler.GetSelectedPupilsAsync(userId))
                .ToList();

        return allSelectedPupils;
    }
}
