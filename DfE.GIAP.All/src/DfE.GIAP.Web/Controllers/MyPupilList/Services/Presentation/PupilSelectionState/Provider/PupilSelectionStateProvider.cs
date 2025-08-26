using DfE.GIAP.Web.Controllers.MyPupilList.Services.Presentation.PupilSelectionState.Provider.DataTransferObjects;
using DfE.GIAP.Web.Providers.Session;

namespace DfE.GIAP.Web.Controllers.MyPupilList.Services.Presentation.PupilSelectionState.Provider;

public sealed class PupilSelectionStateProvider : IPupilSelectionStateProvider
{
    private static string SessionKey => nameof(PupilSelectionStateDto);
    private readonly ISessionProvider _sessionProvider;

    public PupilSelectionStateProvider(
        ISessionProvider sessionProvider)
    {
        _sessionProvider = sessionProvider;
    }

    public PupilsSelectionState GetState()
    {

        PupilSelectionStateDto stateDtoFromSession =
            _sessionProvider.ContainsSessionKey(SessionKey) ?
                _sessionProvider.GetSessionValueOrDefault<PupilSelectionStateDto>(SessionKey) :
                    new();

        PupilsSelectionState pupilSelectionState = PupilsSelectionState.FromDto(stateDtoFromSession);
        return pupilSelectionState;
    }

    public void UpdateState(PupilsSelectionState state)
    {
        PupilSelectionStateDto updatedStateAsDto = state.ToDto();
        _sessionProvider.SetSessionValue(SessionKey, updatedStateAsDto);
    }
}
