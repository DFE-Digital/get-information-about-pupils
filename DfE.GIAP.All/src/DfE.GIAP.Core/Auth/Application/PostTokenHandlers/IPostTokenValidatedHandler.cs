using Microsoft.Extensions.DependencyInjection;

namespace DfE.GIAP.Core.Auth.Application.PostTokenHandlers;

public interface IPostTokenValidatedHandler
{
    Task HandleAsync(TokenAuthorisationContext context);
}

public class PostTokenHandlerBuilder
{
    private readonly IEnumerable<IPostTokenValidatedHandler> _allHandlers;
    private readonly List<IPostTokenValidatedHandler> _ordered = new();

    public PostTokenHandlerBuilder(IEnumerable<IPostTokenValidatedHandler> allHandlers)
    {
        _allHandlers = allHandlers;
    }

    public PostTokenHandlerBuilder StartWith<THandler>() where THandler : IPostTokenValidatedHandler
    {
        _ordered.Add(_allHandlers.OfType<THandler>().Single());
        return this;
    }

    public PostTokenHandlerBuilder Then<THandler>() where THandler : IPostTokenValidatedHandler
    {
        _ordered.Add(_allHandlers.OfType<THandler>().Single());
        return this;
    }

    public IReadOnlyList<IPostTokenValidatedHandler> Build() => _ordered;
}
