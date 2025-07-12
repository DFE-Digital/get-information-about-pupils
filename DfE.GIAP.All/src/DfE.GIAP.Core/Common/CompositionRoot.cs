using DfE.Data.ComponentLibrary.Infrastructure.Persistence.CosmosDb;
using DfE.GIAP.Core.Common.Application.TextSanitiser.Abstraction.Handler;
using DfE.GIAP.Core.Common.Application.TextSanitiser.Handler;
using DfE.GIAP.Core.Common.Application.TextSanitiser.Sanitiser;
using Microsoft.Extensions.DependencyInjection;

namespace DfE.GIAP.Core.Common;
public static class CompositionRoot
{
    public static IServiceCollection AddFeaturesSharedDependencies(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddCosmosDbDependencies();
        services.AddSingleton<ITextSanitiserHandler, TextSanitiserHandler>();
        return services;
    }
}
