using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.MyPupils.Application.Repositories;
using DfE.GIAP.Core.MyPupils.Application.Search.Extensions;
using DfE.GIAP.Core.MyPupils.Application.Search.Options;
using DfE.GIAP.Core.MyPupils.Application.Search.Provider;
using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils;
using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils.DataTransferObjects;
using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils.Mapper;
using DfE.GIAP.Core.MyPupils.Application.UseCases.AddPupilsToMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.Read;
using DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.Write;
using DfE.GIAP.Core.MyPupils.Infrastructure.Search;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DfE.GIAP.Core.MyPupils;
public static class CompositionRoot
{
    public static IServiceCollection AddMyPupilsDependencies(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services
            .AddMyPupilsApplication()
            .AddMyPupilsInfrastructure();

        return services;
    }

    private static IServiceCollection AddMyPupilsApplication(this IServiceCollection services)
    {
        services
            .AddScoped<IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>, GetMyPupilsUseCase>()
            .AddScoped<IUseCaseRequestOnly<AddPupilsToMyPupilsRequest>, AddPupilsToMyPupilsUseCase>()
            .AddScoped<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>, DeletePupilsFromMyPupilsUseCase>()
            .AddSingleton<IMapper<Pupil, MyPupilModel>, MapPupilToMyPupilModelMapper>()
            .AddScoped<IAggregatePupilsForMyPupilsApplicationService, AggregatePupilsForMyPupilsApplicationService>()
            .AddSingleton<IMapper<AzureIndexEntityWithPupilType, Pupil>, MapDecoratedSearchIndexDtoToPupilMapper>();

        return services;
    }

    private static IServiceCollection AddMyPupilsInfrastructure(this IServiceCollection services)
    {
        services
            .AddScoped<IMyPupilsReadOnlyRepository, CosmosDbMyPupilsReadOnlyRepository>()
            .AddScoped<IMyPupilsWriteOnlyRepository, CosmosDbMyPupilsWriteOnlyRepository>()
            .AddSingleton<IMapper<MyPupilsAggregate, MyPupilsDocumentDto>, MyPupilsAggregateToMyPupilsDocumentDtoMapper>()
            .AddMyPupilsInfrastructureSearch();

        return services;
    }

    private static IServiceCollection AddMyPupilsInfrastructureSearch(this IServiceCollection services)
    {
        // Temporary Search Options
        services.AddOptions<SearchIndexOptions>()
            .Configure<IConfiguration>((options, config) =>
            {
                config.GetSection(nameof(SearchIndexOptions)).Bind(options);
            })
            .Validate(
                (options) => !string.IsNullOrEmpty(options.Key), $"{nameof(SearchIndexOptions)}.Key must not be null or empty.")
            .Validate(
                (options) => !string.IsNullOrEmpty(options.Url) && Uri.TryCreate(options.Url, UriKind.Absolute, out _), $"{nameof(SearchIndexOptions)}.Url must not be null or empty.")
            .Validate(
                (options) => options.Indexes.Values.All(
                    (indexOption) => !string.IsNullOrEmpty(indexOption.Name)), $"{nameof(SearchIndexOptions)}.IndexOption has an empty IndexName.")
            .ValidateOnStart();


        // Temporary SearchClients
        services.AddSearchClients();

        // Temporary SearchClientProvider
        services.AddSingleton<ISearchClientProvider, SearchClientProvider>();
        return services;
    }

}
