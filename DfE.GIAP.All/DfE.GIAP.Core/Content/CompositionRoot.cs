using DfE.Data.ComponentLibrary.Infrastructure.Persistence.CosmosDb;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Common.Infrastructure;
using DfE.GIAP.Core.Content.Application.Options;
using DfE.GIAP.Core.Content.Application.Options.Provider;
using DfE.GIAP.Core.Content.Application.Repository;
using DfE.GIAP.Core.Content.Application.UseCases.GetContentByPageKeyUseCase;
using DfE.GIAP.Core.Content.Infrastructure.Repositories;
using DfE.GIAP.Core.Content.Infrastructure.Repositories.Mapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DfE.GIAP.Core.Content;
public static class CompositionRoot
{
    public static IServiceCollection AddContentDependencies(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // both infrastructure and application depend on PageContentOptions
        services.AddOptions<PageContentOptions>()
             .Configure<IServiceProvider>((options, sp) =>
             {
                 IConfiguration configuration = sp.GetRequiredService<IConfiguration>();
                 configuration.GetSection(nameof(PageContentOptions)).Bind(options);
             });
        services.AddSingleton<IPageContentOptionsProvider, PageContentOptionProvider>();

        services
            .RegisterApplicationDependencies()
            .RegisterInfrastructureDependencies();

        return services;
    }

    // Application
    private static IServiceCollection RegisterApplicationDependencies(this IServiceCollection services)
    {
        return services
            .AddScoped<IUseCase<GetContentByPageKeyUseCaseRequest, GetContentByPageKeyUseCaseResponse>, GetContentByPageKeyUseCase>();
    }

    // Infrastructure 
    private static IServiceCollection RegisterInfrastructureDependencies(this IServiceCollection services)
    {
        return services
            .RegisterInfrastructureRepositories()
            .RegisterInfrastructureMappers();
    }

    private static IServiceCollection RegisterInfrastructureRepositories(this IServiceCollection services)
    {
        services.AddScoped<IContentReadOnlyRepository, CosmosDbContentReadOnlyRepository>();
        return services;
    }

    private static IServiceCollection RegisterInfrastructureMappers(this IServiceCollection services)
    {
        return services
            .AddSingleton<IMapper<ContentDTO?, Application.Model.Content>, ContentDTOToContentMapper>();
    }
}
