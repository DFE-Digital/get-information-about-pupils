using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.v2.Evaluator.Options;

namespace DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.v2.Evaluator;
public interface IEvaluatorV2<TIn>
{
    ValueTask EvaluateAsync(TIn input, EvaluationOptions options, CancellationToken ctx = default);
}

public interface IEvaluatorV2<TIn, TOut>
{
    ValueTask<TOut> EvaluateAsync(TIn input, EvaluationOptions options, CancellationToken ctx = default);
}
