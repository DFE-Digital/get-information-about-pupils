using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Web.Features.MyPupils.SelectionState;
using DfE.GIAP.Web.Features.MyPupils.SelectionState.Provider.DataTransferObjects;
using DfE.GIAP.Web.Providers.Session;

namespace DfE.GIAP.Web.Features.MyPupils.SelectionState.Provider;

public sealed class MyPupilsPupilSelectionStateSessionProvider : IMyPupilsPupilSelectionStateProvider
{
    private static string SessionKey => nameof(MyPupilsPupilSelectionStateDto);
    private readonly ISessionProvider _sessionProvider;
    private readonly IMapper<MyPupilsPupilSelectionStateDto, MyPupilsPupilSelectionState> _fromDtoMapper;
    private readonly IMapper<MyPupilsPupilSelectionState, MyPupilsPupilSelectionStateDto> _toDtoMapper;

    public MyPupilsPupilSelectionStateSessionProvider(
        ISessionProvider sessionProvider,
        IMapper<MyPupilsPupilSelectionStateDto, MyPupilsPupilSelectionState> fromDtoMapper,
        IMapper<MyPupilsPupilSelectionState, MyPupilsPupilSelectionStateDto> toDtoMapper)
    {
        ArgumentNullException.ThrowIfNull(sessionProvider);
        _sessionProvider = sessionProvider;

        ArgumentNullException.ThrowIfNull(fromDtoMapper);
        _fromDtoMapper = fromDtoMapper;

        ArgumentNullException.ThrowIfNull(toDtoMapper);
        _toDtoMapper = toDtoMapper;
    }

    public MyPupilsPupilSelectionState GetState()
    {

        MyPupilsPupilSelectionStateDto stateDtoFromSession =
            _sessionProvider.ContainsSessionKey(SessionKey) ?
                _sessionProvider.GetSessionValueOrDefault<MyPupilsPupilSelectionStateDto>(SessionKey) :
                    new();

        MyPupilsPupilSelectionState pupilSelectionState = _fromDtoMapper.Map(stateDtoFromSession);
        return pupilSelectionState;
    }

    public void SetState(MyPupilsPupilSelectionState state)
    {
        MyPupilsPupilSelectionStateDto updatedStateAsDto = _toDtoMapper.Map(state);
        _sessionProvider.SetSessionValue(SessionKey, updatedStateAsDto);
    }
}
