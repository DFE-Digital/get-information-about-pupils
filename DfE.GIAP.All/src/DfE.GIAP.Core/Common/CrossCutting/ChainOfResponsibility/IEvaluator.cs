namespace DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility;
public interface IEvaluator<TIn>
{
    void Evaluate(TIn input);
}
