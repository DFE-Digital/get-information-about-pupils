namespace DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.CommandHandlers;
public interface IEvaluationHandler<TIn>
{
    void Evaluate(TIn input);
}
