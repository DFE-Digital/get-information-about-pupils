namespace DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.CommandHandler;
public interface IChainedCommandHandler<TIn> : ICommandHandler<TIn>
{
    void ChainNext(ICommandHandler<TIn> handler);
}
