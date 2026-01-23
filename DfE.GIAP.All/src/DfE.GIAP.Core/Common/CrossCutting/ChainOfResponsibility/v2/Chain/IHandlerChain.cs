using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.v1;
using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.v2.Handlers;

namespace DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.v2.Composition;

public interface IHandlerChain<TRequest, out THandler>
    where THandler : IEvaluationHandlerV2<TRequest>
{
    IReadOnlyList<THandler> Handlers { get; }
}

public interface IHandlerChain<TRequest, TResponse, out THandler>
    where THandler : IEvaluationHandlerV2<TRequest, TResponse>
{
    IReadOnlyList<THandler> Handlers { get; }
}
