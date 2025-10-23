using DfE.Data.ComponentLibrary.Infrastructure.Persistence.CosmosDb;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.NewsArticles.Application.Models;
using DfE.GIAP.Core.NewsArticles.Application.Repositories;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.CreateNewsArticle;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.DeleteNewsArticle;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticleById;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticles;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.UpdateNewsArticle;
using DfE.GIAP.Core.NewsArticles.Infrastructure.Repositories;
using DfE.GIAP.Core.NewsArticles.Infrastructure.Repositories.DataTransferObjects;
using DfE.GIAP.Core.NewsArticles.Infrastructure.Repositories.Mappers;
using Microsoft.Extensions.DependencyInjection;

namespace DfE.GIAP.Core.NewsArticles;

public static class CompositionRoot
{
    public static IServiceCollection AddNewsArticleDependencies(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services
            .RegisterApplicationDependencies()
            .RegisterInfrastructureDependencies()
            .AddCosmosDbDependencies();
    }

    // Application
    private static IServiceCollection RegisterApplicationDependencies(this IServiceCollection services)
    {
        return services
            .RegisterApplicationUseCases()
            .AddSingleton<IMapper<UpdateNewsArticlesRequestProperties, NewsArticle>, UpdateNewsArticlesRequestPropertiesMapperToNewsArticle>();
    }

    private static IServiceCollection RegisterApplicationUseCases(this IServiceCollection services)
    {
        return services
            .AddScoped<IUseCase<GetNewsArticlesRequest, GetNewsArticlesResponse>, GetNewsArticlesUseCase>()
            .AddScoped<IUseCase<GetNewsArticleByIdRequest, GetNewsArticleByIdResponse>, GetNewsArticleByIdUseCase>()
            .AddScoped<IUseCaseRequestOnly<CreateNewsArticleRequest>, CreateNewsArticleUseCase>()
            .AddScoped<IUseCaseRequestOnly<DeleteNewsArticleRequest>, DeleteNewsArticleUseCase>()
            .AddScoped<IUseCaseRequestOnly<UpdateNewsArticleRequest>, UpdateNewsArticleUseCase>();
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
            .AddScoped<INewsArticleReadOnlyRepository, CosmosDbNewsArticleReadOnlyRepository>()
            .AddScoped<INewsArticleWriteOnlyRepository, CosmosDbNewsArticleWriteOnlyRepository>();
    }

    private static IServiceCollection RegisterInfrastructureMappers(this IServiceCollection services)
    {
        return services
            .AddScoped<IMapper<NewsArticleDto, NewsArticle>, NewsArticleDtoToEntityMapper>()
            .AddScoped<IMapper<NewsArticle, NewsArticleDto>, NewsArticleEntityToDtoMapper>();
    }
}
