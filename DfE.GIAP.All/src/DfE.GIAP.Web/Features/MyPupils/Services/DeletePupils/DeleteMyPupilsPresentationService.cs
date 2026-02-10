using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection.ClearPupilSelections;
using DfE.GIAP.Web.Features.MyPupils.Services.GetSelectedPupilUpns;

namespace DfE.GIAP.Web.Features.MyPupils.Services.DeletePupils;

public class DeleteMyPupilsPresentationService : IDeleteMyPupilsPresentationService
{
    private readonly IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest> _deletePupilsUseCase;
    private readonly IGetSelectedPupilsUniquePupilNumbersPresentationService _getSelectionsHandler;
    private readonly IClearMyPupilsPupilSelectionsHandler _clearSelectionsHandler;

    public DeleteMyPupilsPresentationService(
        IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest> deletePupilsUseCase,
        IGetSelectedPupilsUniquePupilNumbersPresentationService getSelectionsHandler,
        IClearMyPupilsPupilSelectionsHandler clearSelectionsHandler)
    {
        ArgumentNullException.ThrowIfNull(getSelectionsHandler);
        _getSelectionsHandler = getSelectionsHandler;

        ArgumentNullException.ThrowIfNull(deletePupilsUseCase);
        _deletePupilsUseCase = deletePupilsUseCase;

        ArgumentNullException.ThrowIfNull(clearSelectionsHandler);
        _clearSelectionsHandler = clearSelectionsHandler;
    }

    public async Task DeletePupilsAsync(string userId, IEnumerable<string> selectedPupils)
    {
        List<string> selectedPupilsToDelete = selectedPupils?.ToList() ?? [];

        // Enrich SelectedPupils with all other selected pupils
        selectedPupilsToDelete.AddRange(
            await _getSelectionsHandler.GetSelectedPupilsAsync(userId));

        await _deletePupilsUseCase.HandleRequestAsync(
            new DeletePupilsFromMyPupilsRequest(
                UserId: userId,
                DeletePupilUpns: selectedPupilsToDelete.Distinct()));

        _clearSelectionsHandler.Handle();
    }
}
