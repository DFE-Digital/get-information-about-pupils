namespace DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.v1;
public interface IEvaluator<TIn>
{
    void Evaluate(TIn input);
}
