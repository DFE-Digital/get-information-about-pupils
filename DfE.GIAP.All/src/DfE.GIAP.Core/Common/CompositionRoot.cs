using DfE.Data.ComponentLibrary.Infrastructure.Persistence.CosmosDb;
using DfE.GIAP.Core.Common.Application.TextSanitiser.Handlers;
using DfE.GIAP.Core.Common.Application.TextSanitiser.Invoker;
using DfE.GIAP.Core.Common.CrossCutting.Logging;
using DfE.GIAP.Core.Search.Application.Adapters;
using DfE.GIAP.Core.Search.Application.Services.SearchByIdentifier;
using DfE.GIAP.Core.Search.Application.Services.SearchByName;
using DfE.GIAP.Core.Search.Infrastructure.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DfE.GIAP.Core.Common;
public static class CompositionRoot
{
    public static IServiceCollection AddFeaturesSharedDependencies(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services
            .AddCosmosDbDependencies()
            .AddCrossCuttingLoggingDependencies();

        // open generics for Search
        services.TryAddScoped(typeof(ISearchServiceAdapter<,>), typeof(AzureSearchServiceAdaptor<,>));
        services.TryAddScoped(typeof(ISearchLearnerByNameService<>), typeof(SearchLearnerByNameService<>));
        services.TryAddScoped(typeof(ISearchLearnersByIdentifierService<>), typeof(SearchLearnerByIdentifierService<>));

        services.TryAddEnumerable(new ServiceDescriptor(typeof(IEnumerable<ITextSanitiserHandler>), Array.Empty<ITextSanitiserHandler>()));
        services.TryAddSingleton<ITextSanitiser, TextSanitiser>();

        return services;
    }
}
