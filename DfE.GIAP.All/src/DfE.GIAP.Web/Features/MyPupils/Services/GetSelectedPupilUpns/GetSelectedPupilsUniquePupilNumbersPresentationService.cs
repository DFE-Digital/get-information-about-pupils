using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection.Operations;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection.Operations.GetPupilSelections;

namespace DfE.GIAP.Web.Features.MyPupils.Services.GetSelectedPupilIdentifiers;

public sealed class GetSelectedPupilsUniquePupilNumbersPresentationService : IGetSelectedPupilsUniquePupilNumbersPresentationService
{
    private readonly IGetMyPupilsPupilSelectionProvider _getMyPupilsStateProvider;
    private readonly IUseCase<GetMyPupilsRequest, GetMyPupilsResponse> _getMyPupilsUseCase;

    public GetSelectedPupilsUniquePupilNumbersPresentationService(
        IGetMyPupilsPupilSelectionProvider getMyPupilsStateProvider,
        IUseCase<GetMyPupilsRequest, GetMyPupilsResponse> getMyPupilsUseCase)
    {
        ArgumentNullException.ThrowIfNull(getMyPupilsStateProvider);
        _getMyPupilsStateProvider = getMyPupilsStateProvider;

        ArgumentNullException.ThrowIfNull(getMyPupilsUseCase);
        _getMyPupilsUseCase = getMyPupilsUseCase;
    }

    public async Task<IEnumerable<string>> GetSelectedPupilsAsync(string userId)
    {
        MyPupilsPupilSelectionState state = _getMyPupilsStateProvider.GetPupilSelections();

        if (state.Mode == SelectionMode.All)
        {
            GetMyPupilsResponse response =
                await _getMyPupilsUseCase.HandleRequestAsync(
                    new GetMyPupilsRequest(userId));

            return response.MyPupils.Identifiers.Except(
                    state.GetDeselectedExceptions());
        }

        return state.GetManualSelections();
    }
}
