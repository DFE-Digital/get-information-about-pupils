using DfE.Data.ComponentLibrary.Infrastructure.Persistence.CosmosDb;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Common.Infrastructure;
using DfE.GIAP.Core.Content.Application.Repository;
using DfE.GIAP.Core.Content.Application.UseCases.GetContentById;
using DfE.GIAP.Core.Content.Application.UseCases.GetContentByKey;
using DfE.GIAP.Core.Content.Application.UseCases.GetMultipleContentByKeys;
using DfE.GIAP.Core.Content.Infrastructure.Repositories;
using DfE.GIAP.Core.Content.Infrastructure.Repositories.Mapper;
using Microsoft.Extensions.DependencyInjection;

namespace DfE.GIAP.Core.Content;
public static class CompositionRoot
{
    public static IServiceCollection AddContentDependencies(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services
            .RegisterApplicationDependencies()
            .RegisterInfrastructureDependencies();
    }

    // Application
    private static IServiceCollection RegisterApplicationDependencies(this IServiceCollection services)
    {
        return services
            .AddScoped<IUseCase<GetContentByKeyUseCaseRequest, GetContentByKeyUseCaseResponse>, GetContentByKeyUseCase>()
            .AddScoped<IUseCase<GetMultipleContentByKeysUseCaseRequest, GetMultipleContentByKeysUseCaseResponse>, GetMultipleContentByKeysUseCase>();
    }

    // Infrastructure 
    private static IServiceCollection RegisterInfrastructureDependencies(this IServiceCollection services)
    {
        return services
            .AddCosmosDbDependencies()
            .RegisterInfrastructureRepositories()
            .RegisterInfrastructureMappers();
    }

    private static IServiceCollection RegisterInfrastructureRepositories(this IServiceCollection services)
    {
        services.AddTemporaryCosmosClient();
        services.AddScoped<IContentReadOnlyRepository, CosmosDbContentReadOnlyRepository>();
        return services;
    }

    private static IServiceCollection RegisterInfrastructureMappers(this IServiceCollection services)
    {
        return services
            .AddSingleton<IMapper<ContentDTO, Application.Model.Content>, ContentDTOToContentMapper>();
    }
}
