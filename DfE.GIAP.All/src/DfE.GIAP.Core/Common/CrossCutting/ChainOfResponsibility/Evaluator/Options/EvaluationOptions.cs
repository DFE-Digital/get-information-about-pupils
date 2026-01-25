namespace DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.Evaluator.Options;
public sealed class EvaluationOptions
{
    // Should we ever Represent executing SPECIFIC handlers in the chain...?
    // TODO we should model execution behaviour, so that it can be Validated and informs strategy composition.
    // e.g.
    // - Should we choose the first successful response available
    // - should we error on failure states?
    public ChainExecutionMode Mode { get; init; } = ChainExecutionMode.ChainOfResponsibility;
    // TraceIdentifier?
    // Observers / PrePost handlers enabling custom hooks between handlers List<Func<Task?
    // CancellationToken in here or at end of evaluator contract
}
