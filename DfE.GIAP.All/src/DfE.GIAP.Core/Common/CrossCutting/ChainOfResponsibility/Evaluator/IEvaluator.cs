using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.Evaluator.Options;

namespace DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.Evaluator;
public interface IEvaluator<TIn>
{
    ValueTask EvaluateAsync(TIn input, EvaluationOptions? options = null, CancellationToken ctx = default);
}
