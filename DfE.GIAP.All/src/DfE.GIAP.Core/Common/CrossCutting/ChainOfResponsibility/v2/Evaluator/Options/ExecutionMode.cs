namespace DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.v2.Evaluator.Options;

public enum ExecutionMode
{
    FirstSuccess,   // CoR
    UntilFailure,   // stop on first failure
    AlwaysAll       // Handle even if CanHandle is false
}
