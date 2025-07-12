using DfE.Data.ComponentLibrary.Infrastructure.Persistence.CosmosDb;
using DfE.GIAP.Core.Common.Application.TextSanitiser.Invoker;
using Microsoft.Extensions.DependencyInjection;

namespace DfE.GIAP.Core.Common;
public static class CompositionRoot
{
    public static IServiceCollection AddFeaturesSharedDependencies(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddCosmosDbDependencies();
        services.AddSingleton<ITextSanitiserInvoker, TextSanitisationInvoker>();
        return services;
    }
}
