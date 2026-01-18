namespace DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.v2.Evaluator.Options;
public sealed class EvaluationOptions
{
    public ExecutionMode ExecutionMode { get; init; } = ExecutionMode.FirstSuccess;
}
