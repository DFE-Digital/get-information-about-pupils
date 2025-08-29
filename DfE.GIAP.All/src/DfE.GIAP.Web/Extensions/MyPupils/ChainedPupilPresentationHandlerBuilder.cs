using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Web.Features.MyPupils.Handlers.GetPaginatedMyPupils.PresentationHandlers;
using DfE.GIAP.Web.Features.MyPupils.PresentationState;

namespace DfE.GIAP.Web.Extensions.MyPupils;

// TODO could genericise for any T handler
public class ChainedEvaluationPupilPresentationHandler : IPupilDtosPresentationHandler
{
    private readonly IPupilDtosPresentationHandler _current;
    private ChainedEvaluationPupilPresentationHandler? _next;

    public ChainedEvaluationPupilPresentationHandler(IPupilDtosPresentationHandler current)
    {
        _current = current;
    }

    public ChainedEvaluationPupilPresentationHandler ChainNext(IPupilDtosPresentationHandler next)
    {
        if (_next is null)
        {
            _next = new ChainedEvaluationPupilPresentationHandler(next);
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
