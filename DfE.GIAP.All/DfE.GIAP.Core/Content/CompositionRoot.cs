using DfE.Data.ComponentLibrary.Infrastructure.Persistence.CosmosDb;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Common.Infrastructure;
using DfE.GIAP.Core.Content.Application.Options;
using DfE.GIAP.Core.Content.Application.Repository;
using DfE.GIAP.Core.Content.Application.UseCases.GetContentByPageKeyUseCase;
using DfE.GIAP.Core.Content.Infrastructure.Repositories;
using DfE.GIAP.Core.Content.Infrastructure.Repositories.Mapper;
using DfE.GIAP.Core.Content.Infrastructure.Repositories.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DfE.GIAP.Core.Content;
public static class CompositionRoot
{
    public static IServiceCollection AddContentDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services
            .RegisterApplicationDependencies(configuration)
            .RegisterInfrastructureDependencies(configuration);
    }

    // Application
    private static IServiceCollection RegisterApplicationDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddScoped<IUseCase<GetContentByPageKeyUseCaseRequest, GetContentByPageKeyUseCaseResponse>, GetContentByPageKeyUseCase>();
        services.Configure<PageContentOptions>(
            (options) => configuration.GetSection(nameof(PageContentOptions)).Bind(options));

        return services;
    }

    // Infrastructure 
    private static IServiceCollection RegisterInfrastructureDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddCosmosDbDependencies()
            .RegisterInfrastructureRepositories()
            .RegisterInfrastructureMappers()
            .Configure<ContentRepositoryOptions>(
                (options) => configuration.GetSection(nameof(ContentRepositoryOptions)).Bind(options));

    }

    private static IServiceCollection RegisterInfrastructureRepositories(this IServiceCollection services)
    {
        services.AddTemporaryCosmosClient();
        services.AddScoped<IContentReadOnlyRepository, TempCosmosDbContentReadOnlyRepository>();
        return services;
    }

    private static IServiceCollection RegisterInfrastructureMappers(this IServiceCollection services)
    {
        return services
            .AddSingleton<IMapper<ContentDTO?, Application.Model.Content>, ContentDTOToContentMapper>();
    }
}
