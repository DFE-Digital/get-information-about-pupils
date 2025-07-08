using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Contents.Application.Models;
using DfE.GIAP.Core.Contents.Application.Options;
using DfE.GIAP.Core.Contents.Application.Options.Provider;
using DfE.GIAP.Core.Contents.Application.Repositories;
using DfE.GIAP.Core.Contents.Application.UseCases.GetContentByPageKeyUseCase;
using DfE.GIAP.Core.Contents.Infrastructure.Repositories;
using DfE.GIAP.Core.Contents.Infrastructure.Repositories.Mapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DfE.GIAP.Core.Contents;

/// <summary>
/// Provides extension methods to register application and infrastructure dependencies for the Content module.
/// </summary>
public static class CompositionRoot
{
    /// <summary>
    /// Registers all dependencies required by the Content module.
    /// </summary>
    /// <param name="services">The service collection to register with.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddContentDependencies(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services
            .RegisterApplicationDependencies()
            .RegisterInfrastructureDependencies();

        return services;
    }

    /// <summary>
    /// Registers application-layer services such as use cases and configuration providers.
    /// </summary>
    private static IServiceCollection RegisterApplicationDependencies(this IServiceCollection services)
    {
        services.AddScoped<IUseCase<GetContentByPageKeyUseCaseRequest, GetContentByPageKeyUseCaseResponse>, GetContentByPageKeyUseCase>();
        services.AddOptions<PageContentOptions>().Configure<IServiceProvider>((options, sp) =>
        {
            IConfiguration configuration = sp.GetRequiredService<IConfiguration>();
            configuration.GetSection(nameof(PageContentOptions)).Bind(options);
        });
        services.AddSingleton<IPageContentOptionsProvider, PageContentOptionProvider>();
        return services;
    }

    /// <summary>
    /// Registers infrastructure-layer services such as repositories and mappers.
    /// </summary>
    private static IServiceCollection RegisterInfrastructureDependencies(this IServiceCollection services)
    {
        return services
            .RegisterInfrastructureRepositories()
            .RegisterInfrastructureMappers();
    }

    /// <summary>
    /// Registers repository implementations for the infrastructure layer.
    /// </summary>
    private static IServiceCollection RegisterInfrastructureRepositories(this IServiceCollection services)
    {
        services.AddScoped<IContentReadOnlyRepository, CosmosDbContentReadOnlyRepository>();
        return services;
    }

    /// <summary>
    /// Registers mapper implementations for the infrastructure layer.
    /// </summary>
    private static IServiceCollection RegisterInfrastructureMappers(this IServiceCollection services)
    {
        return services
            .AddSingleton<IMapper<ContentDto?, Content>, ContentDtoToContentMapper>();
    }
}
