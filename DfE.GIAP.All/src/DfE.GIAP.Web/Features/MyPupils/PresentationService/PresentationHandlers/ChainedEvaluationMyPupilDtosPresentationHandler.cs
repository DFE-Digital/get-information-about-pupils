using DfE.GIAP.Web.Features.MyPupils.PresentationService.Models;

namespace DfE.GIAP.Web.Features.MyPupils.PresentationService.PresentationHandlers;

// TODO could genericise for any ChainedHandler
public class ChainedEvaluationMyPupilDtosPresentationHandler : IMyPupilsPresentationModelHandler
{
    private readonly IMyPupilsPresentationModelHandler _current;
    private ChainedEvaluationMyPupilDtosPresentationHandler _next;

    public ChainedEvaluationMyPupilDtosPresentationHandler(IMyPupilsPresentationModelHandler head)
    {
        _current = head;
    }

    public ChainedEvaluationMyPupilDtosPresentationHandler ChainNext(IMyPupilsPresentationModelHandler next)
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

    public MyPupilsPresentationPupilModels Handle(MyPupilsPresentationPupilModels myPupils, MyPupilsState state)
    {
        MyPupilsPresentationPupilModels result = _current.Handle(myPupils, state);
        return _next?.Handle(result, state) ?? result;
    }
}
