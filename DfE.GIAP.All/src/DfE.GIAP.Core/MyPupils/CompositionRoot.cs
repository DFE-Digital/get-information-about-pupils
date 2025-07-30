using Azure;
using Azure.Search.Documents;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.Search.Options;
using DfE.GIAP.Core.MyPupils.Application.Search.Options.Extensions;
using DfE.GIAP.Core.MyPupils.Application.Search.Provider;
using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils;
using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils.Mapper;
using DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Request;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.User.Application.Repository;
using DfE.GIAP.Core.User.Infrastructure.Repository;
using DfE.GIAP.Core.User.Infrastructure.Repository.Dtos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
            
            .AddScoped<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>, DeletePupilsFromMyPupilsUseCase>()
            .AddScoped<IAggregatePupilsForMyPupilsApplicationService, TempAggregatePupilsForMyPupilsApplicationService>()
            .AddScoped<IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>, GetMyPupilsUseCase>()
            .AddSingleton<IMapper<Pupil, PupilDto>, MapPupilToPupilDtoMapper>()
            .AddSingleton<IMapper<MappableLearner, Pupil>, MapMappableLearnerToPupilMapper>();

        return services;
    }

    private static IServiceCollection AddMyPupilsInfrastructure(this IServiceCollection services)
    {
        services
            .AddScoped<IUserReadOnlyRepository, CosmosDbUserReadOnlyRepository>()
            .AddScoped<IUserWriteRepository, CosmosDbUserWriteRepository>()
            .AddSingleton<IMapper<UserDto, User.Application.User>, MapUserProfileDtoToUserMapper>();

        services.AddMyPupilsInfrastructureSearch();
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
                (options) => options.IndexOptions.All(
                    (indexOption) => !string.IsNullOrEmpty(indexOption.IndexName)), $"{nameof(SearchIndexOptions)}.IndexOption has an empty IndexName.")
            .ValidateOnStart();


        // Temporary SearchClients
        services.AddSingleton<SearchClient>(sp =>
        {
            SearchIndexOptions options = sp.GetRequiredService<IOptions<SearchIndexOptions>>().Value;
            IndexOptions indexOptions = options.GetIndexOptionsByName("npd");

            SearchClient searchClient = new(
                new Uri(options.Url),
                indexOptions.IndexName,
                new AzureKeyCredential(options.Key));
            return searchClient;
        });

        services.AddSingleton<SearchClient>(sp =>
        {
            SearchIndexOptions options = sp.GetRequiredService<IOptions<SearchIndexOptions>>().Value;
            IndexOptions indexOptions = options.GetIndexOptionsByName("pupil-premium");

            SearchClient searchClient = new(
                new Uri(options.Url),
                indexOptions.IndexName,
                new AzureKeyCredential(options.Key));
            return searchClient;
        });

        // Temporary SearchClientProvider
        services.AddSingleton<ISearchClientProvider, SearchClientProvider>();

        return services;
    }
}
