namespace DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.v1;
public interface IChainedEvaluationHandler<TIn> : IEvaluationHandler<TIn>
{
    void ChainNext(IEvaluationHandler<TIn> handler);
}

public interface IChainedEvaluationHandler<TIn, TOut> : IEvaluationHandler<TIn, TOut>
{
    void ChainNext(IEvaluationHandler<TIn, TOut> handler);
}
