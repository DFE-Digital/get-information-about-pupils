namespace DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility;
public interface IChainedEvaluationHandler<TIn> : IEvaluationHandler<TIn>
{
    void ChainNext(IEvaluationHandler<TIn> handler);
}
