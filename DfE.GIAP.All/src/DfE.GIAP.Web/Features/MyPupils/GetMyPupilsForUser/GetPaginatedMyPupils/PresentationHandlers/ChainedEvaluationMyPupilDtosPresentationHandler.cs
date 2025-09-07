using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Web.Features.MyPupils.State.Presentation;

namespace DfE.GIAP.Web.Features.MyPupils.GetMyPupilsForUser.GetPaginatedMyPupils.PresentationHandlers;

// TODO could genericise for any ChainedHandler
public class ChainedEvaluationMyPupilDtosPresentationHandler : IMyPupilDtosPresentationHandler
{
    private readonly IMyPupilDtosPresentationHandler _current;
    private ChainedEvaluationMyPupilDtosPresentationHandler _next;

    public ChainedEvaluationMyPupilDtosPresentationHandler(IMyPupilDtosPresentationHandler current)
    {
        _current = current;
    }

    public ChainedEvaluationMyPupilDtosPresentationHandler ChainNext(IMyPupilDtosPresentationHandler next)
    {
        if (_next is null)
        {
            _next = new ChainedEvaluationMyPupilDtosPresentationHandler(next);
        }
        else
        {
            _next.ChainNext(next);
        }

        return this;
    }

    public MyPupilDtos Handle(MyPupilDtos pupils, MyPupilsPresentationState state)
    {
        MyPupilDtos result = _current.Handle(pupils, state);
        return _next?.Handle(result, state) ?? result;
    }
}
