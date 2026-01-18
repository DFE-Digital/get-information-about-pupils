namespace DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.v1;
public sealed class Evaluator<TIn> : IEvaluator<TIn>
{
    private readonly IEvaluationHandler<TIn> _handler;

    public Evaluator(IEvaluationHandler<TIn> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        _handler = handler;
    }
    public void Evaluate(TIn input) => _handler.Handle(input);
}
