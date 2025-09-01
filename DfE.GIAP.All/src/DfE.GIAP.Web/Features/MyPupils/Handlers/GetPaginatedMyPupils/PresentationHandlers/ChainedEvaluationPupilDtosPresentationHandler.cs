using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Web.Features.MyPupils.State.Presentation;

namespace DfE.GIAP.Web.Features.MyPupils.Handlers.GetPaginatedMyPupils.PresentationHandlers;

// TODO could genericise for any ChainedHandler
public class ChainedEvaluationPupilDtosPresentationHandler : IPupilDtosPresentationHandler
{
    private readonly IPupilDtosPresentationHandler _current;
    private ChainedEvaluationPupilDtosPresentationHandler? _next;

    public ChainedEvaluationPupilDtosPresentationHandler(IPupilDtosPresentationHandler current)
    {
        _current = current;
    }

    public ChainedEvaluationPupilDtosPresentationHandler ChainNext(IPupilDtosPresentationHandler next)
    {
        if (_next is null)
        {
            _next = new ChainedEvaluationPupilDtosPresentationHandler(next);
        }
        else
        {
            _next.ChainNext(next);
        }

        return this;
    }

    public PupilDtos Handle(PupilDtos pupils, MyPupilsPresentationState state)
    {
        PupilDtos result = _current.Handle(pupils, state);
        return _next?.Handle(result, state) ?? result;
    }
}
