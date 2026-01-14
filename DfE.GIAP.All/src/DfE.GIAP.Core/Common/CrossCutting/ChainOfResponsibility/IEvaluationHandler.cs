namespace DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility;
public interface IEvaluationHandler<TIn>
{
    bool CanHandle(TIn input);
    void Handle(TIn input);
}
