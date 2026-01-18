namespace DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.v2.Composition;
public interface IHandlerChainBuilder<T>
{
    IHandlerChainBuilder<T> ChainNext(T handler);
    IHandlerChain<T> Build();
}
