namespace DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.v1;
public interface IEvaluationHandler<TIn>
{
    bool CanHandle(TIn input);
    void Handle(TIn input);
}

public interface IEvaluationHandler<TIn, TOut>
{
    bool CanHandle(TIn input);
    TOut Handle(TIn input);
}
