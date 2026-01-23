using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.v1;
using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.v2.Handlers;

namespace DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.v2.Composition;
public interface IHandlerChainBuilder<TInput, THandler>
    where THandler : IEvaluationHandlerV2<TInput>
{
    IHandlerChainBuilder<TInput, THandler> ChainNext(THandler handler);
    IHandlerChain<TInput, THandler> Build();
}

public interface IHandlerChainBuilder<TInput, TOutput, THandler>
    where THandler : IEvaluationHandlerV2<TInput, TOutput>
{
    IHandlerChainBuilder<TInput, TOutput, THandler> ChainNext(THandler handler);
    IHandlerChain<TInput, TOutput, THandler> Build();
}
