namespace DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.CommandHandlers;
public sealed class EvaluationHandler<TIn> : IEvaluationHandler<TIn>
{
    private readonly ICommandHandler<TIn> _handler;

    public EvaluationHandler(ICommandHandler<TIn> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        _handler = handler;
    }
    public void Evaluate(TIn input) => _handler.Handle(input);
}
