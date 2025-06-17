using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Common.Infrastructure;
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
            .RegisterInfrastructureRepositories()
            .RegisterInfrastructureMappers();
    }

    private static IServiceCollection RegisterInfrastructureRepositories(this IServiceCollection services)
    {
        return services
            .AddTemporaryCosmosClient()
            .AddScoped<INewsArticleReadRepository, TempNewsArticleReadRepository>() // TODO: Swap with CosmosNewsArticleReadRepository when ready
            .AddScoped<INewsArticleWriteRepository, TempNewsArticleWriteRepository>(); // TODO: Swap with CosmosNewsArticleWriteRepository when ready
    }

    private static IServiceCollection RegisterInfrastructureMappers(this IServiceCollection services)
    {
        return services
            .AddScoped<IMapper<NewsArticleDTO, NewsArticle>, NewsArticleDtoToEntityMapper>()
            .AddScoped<IMapper<NewsArticle, NewsArticleDTO>, NewsArticleEntityToDtoMapper>();
    }
}
