namespace DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.CommandHandlers;
public interface ICommandHandler<TIn>
{
    bool CanHandle(TIn input);
    void Handle(TIn input);
}
