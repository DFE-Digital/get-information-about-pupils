namespace DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.CommandHandlers;
public interface IChainedCommandHandler<TIn> : ICommandHandler<TIn>
{
    void ChainNext(ICommandHandler<TIn> handler);
}
