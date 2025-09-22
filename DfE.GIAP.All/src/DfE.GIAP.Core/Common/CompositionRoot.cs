using DfE.Data.ComponentLibrary.Infrastructure.Persistence.CosmosDb;
using DfE.GIAP.Core.Common.Application.TextSanitiser.Handlers;
using DfE.GIAP.Core.Common.Application.TextSanitiser.Invoker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DfE.GIAP.Core.Common;
public static class CompositionRoot
{
    public static IServiceCollection AddFeaturesSharedDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddCosmosDbDependencies();
        services.TryAddEnumerable(new ServiceDescriptor(typeof(IEnumerable<ITextSanitiserHandler>), Array.Empty<ITextSanitiserHandler>()));
        services.AddSingleton<ITextSanitiserInvoker, TextSanitisationInvoker>();

        return services;
    }
}
