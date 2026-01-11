namespace DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.CommandHandler;
public interface IEvaluationHandler<TIn>
{
    void Evaluate(TIn input);
}
