using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Request;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Web.Controllers.MyPupilList.PupilSelectionState;
using DfE.GIAP.Web.Controllers.MyPupilList.PupilSelectionState.Provider;

namespace DfE.GIAP.Web.Controllers.MyPupilList.Handlers.GetSelectedPupilsForUserAsync;

public class GetSelectedPupilsForUserHandler : IGetSelectedPupilsForUserHandler
{
    private readonly IUseCase<GetMyPupilsRequest, GetMyPupilsResponse> _getMyPupilsUseCase;
    private readonly IPupilSelectionStateProvider _pupilSelectionStateProvider;

    public GetSelectedPupilsForUserHandler(
        IUseCase<GetMyPupilsRequest, GetMyPupilsResponse> getMyPupilsUseCase,
        IPupilSelectionStateProvider pupilSelectionStateProvider)
    {
        _getMyPupilsUseCase = getMyPupilsUseCase;
        _pupilSelectionStateProvider = pupilSelectionStateProvider;
    }

    public async Task<IEnumerable<string>> GetSelectedPupilsForUserAsync(string userId)
    {
        IPupilsSelectionState pupilsSelectionState = _pupilSelectionStateProvider.GetState();

        if (pupilsSelectionState.IsAllPupilsSelected)
        {
            GetMyPupilsRequest getPupilsRequest = new(userId);
            GetMyPupilsResponse response = await _getMyPupilsUseCase.HandleRequestAsync(getPupilsRequest);
            return response.Pupils.Select(t => t.UniquePupilNumber);
        }

        return pupilsSelectionState.GetSelectedPupils();
    }
}
