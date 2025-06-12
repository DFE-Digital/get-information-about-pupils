using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Options;
using DfE.Data.ComponentLibrary.Infrastructure.Persistence.CosmosDb;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.NewsArticles.Application.Models;
using DfE.GIAP.Core.NewsArticles.Application.Repositories;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.CreateNewsArticle;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticleById;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticles;
using DfE.GIAP.Core.NewsArticles.Infrastructure.Repositories;
using DfE.GIAP.Core.NewsArticles.Infrastructure.Repositories.Mappers;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Core.NewsArticles;

public static class CompositionRoot
{
    public static IServiceCollection AddNewsArticleDependencies(this IServiceCollection services)
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
            .RegisterApplicationUseCases();
    }

    private static IServiceCollection RegisterApplicationUseCases(this IServiceCollection services)
    {
        return services
            .AddScoped<IUseCase<GetNewsArticlesRequest, GetNewsArticlesResponse>, GetNewsArticlesUseCase>()
            .AddScoped<IUseCase<GetNewsArticleByIdRequest, GetNewsArticleByIdResponse>, GetNewsArticleByIdUseCase>()
            .AddScoped<IUseCaseRequestOnly<CreateNewsArticleRequest>, CreateNewsArticleUseCase>();
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
        services
            .AddSingleton<CosmosClient>(sp =>
            {
                IOptions<RepositoryOptions> repositoryOptions = sp.GetRequiredService<IOptions<RepositoryOptions>>();
                RepositoryOptions options = repositoryOptions.Value ?? throw new ArgumentException($"{repositoryOptions.Value} is null");

                if (string.IsNullOrWhiteSpace(options.EndpointUri))
                {
                    throw new ArgumentException("RepositoryOptions.EndpointUri is null or empty");
                }

                if (string.IsNullOrWhiteSpace(options.PrimaryKey))
                {
                    throw new ArgumentException("RepositoryOptions.PrimaryKey is null or empty");
                }

                CosmosClientBuilder cosmosClientBuilder = new(options.EndpointUri, options.PrimaryKey);

                // Check if the environment is local to use Gateway
                string? environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

                if (!string.IsNullOrWhiteSpace(environment) && environment.Equals("Local", StringComparison.OrdinalIgnoreCase))
                {
                    cosmosClientBuilder.WithConnectionModeGateway(); // Use gateway for local running only
                }

                return cosmosClientBuilder.Build();
            });

        services.AddScoped<INewsArticleReadRepository, TempNewsArticleReadRepository>(); // TODO: Swap with CosmosNewsArticleReadRepository when ready
        services.AddScoped<INewsArticleWriteRepository, TempNewsArticleWriteRepository>(); // TODO: Swap with CosmosNewsArticleWriteRepository when ready

        return services;
    }

    private static IServiceCollection RegisterInfrastructureMappers(this IServiceCollection services)
    {
        return services
            .AddScoped<IMapper<NewsArticleDTO, NewsArticle>, NewsArticleDtoToEntityMapper>()
            .AddScoped<IMapper<NewsArticle, NewsArticleDTO>, NewsArticleEntityToDtoMapper>();
    }
}
