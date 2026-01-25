using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.Handlers;

namespace DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.Chain;
public interface IHandlerChainBuilder<TInput, THandler>
    where THandler : IEvaluationHandler<TInput>
{
    IHandlerChainBuilder<TInput, THandler> ChainNext(THandler handler);
    IHandlerChain<TInput, THandler> Build();
}
