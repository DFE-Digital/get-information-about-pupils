namespace DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.CommandHandler;
public interface ICommandHandler<TIn>
{
    bool CanHandle(TIn input);
    void Handle(TIn input);
}
