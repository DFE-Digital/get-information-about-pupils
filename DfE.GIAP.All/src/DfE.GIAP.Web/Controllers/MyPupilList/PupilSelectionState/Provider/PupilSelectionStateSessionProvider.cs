using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Web.Controllers.MyPupilList.PupilSelectionState.Provider.DataTransferObjects;
using DfE.GIAP.Web.Providers.Session;

namespace DfE.GIAP.Web.Controllers.MyPupilList.PupilSelectionState.Provider;

public sealed class PupilSelectionStateSessionProvider : IPupilSelectionStateProvider
{
    private static string SessionKey => nameof(MyPupilsPupilSelectionStateDto);
    private readonly ISessionProvider _sessionProvider;
    private readonly IMapper<MyPupilsPupilSelectionStateDto, IMyPupilsPupilSelectionState> _fromDtoMapper;
    private readonly IMapper<IMyPupilsPupilSelectionState, MyPupilsPupilSelectionStateDto> _toDtoMapper;

    public PupilSelectionStateSessionProvider(
        ISessionProvider sessionProvider,
        IMapper<MyPupilsPupilSelectionStateDto, IMyPupilsPupilSelectionState> fromDtoMapper,
        IMapper<IMyPupilsPupilSelectionState, MyPupilsPupilSelectionStateDto> toDtoMapper)
    {
        ArgumentNullException.ThrowIfNull(sessionProvider);
        _sessionProvider = sessionProvider;

        ArgumentNullException.ThrowIfNull(fromDtoMapper);
        _fromDtoMapper = fromDtoMapper;

        ArgumentNullException.ThrowIfNull(toDtoMapper);
        _toDtoMapper = toDtoMapper;
    }

    public IMyPupilsPupilSelectionState GetState()
    {

        MyPupilsPupilSelectionStateDto stateDtoFromSession =
            _sessionProvider.ContainsSessionKey(SessionKey) ?
                _sessionProvider.GetSessionValueOrDefault<MyPupilsPupilSelectionStateDto>(SessionKey) :
                    new();

        IMyPupilsPupilSelectionState pupilSelectionState = _fromDtoMapper.Map(stateDtoFromSession);
        return pupilSelectionState;
    }

    public void SetState(IMyPupilsPupilSelectionState state)
    {
        MyPupilsPupilSelectionStateDto updatedStateAsDto = _toDtoMapper.Map(state);
        _sessionProvider.SetSessionValue(SessionKey, updatedStateAsDto);
    }
}
