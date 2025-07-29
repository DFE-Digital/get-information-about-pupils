using Azure;
using Azure.Search.Documents;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.Options;
using DfE.GIAP.Core.MyPupils.Application.Options.Extensions;
using DfE.GIAP.Core.MyPupils.Application.Repository.UserAggregate;
using DfE.GIAP.Core.MyPupils.Application.Services;
using DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Request;
using DfE.GIAP.Core.MyPupils.Application.UseCases.Services.AggregatePupilsForMyPupilsDomainService;
using DfE.GIAP.Core.MyPupils.Application.UseCases.Services.AggregatePupilsForMyPupilsDomainService.Client;
using DfE.GIAP.Core.MyPupils.Application.UseCases.Services.AggregatePupilsForMyPupilsDomainService.Mapper;
using DfE.GIAP.Core.MyPupils.Domain.Aggregate;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Infrastructure;
using DfE.GIAP.Core.User.Application.Repository.UserReadRepository;
using DfE.GIAP.Core.User.Infrastructure.Repository;
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
            .AddScoped<IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>, GetMyPupilsUseCase>()
            .AddScoped<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>, DeletePupilsFromMyPupilsUseCase>()
            .AddScoped<IAggregatePupilsForMyPupilsApplicationService, TempAggregatePupilsForMyPupilsApplicationService>()
            .AddSingleton<IMapper<Pupil, PupilDto>, MapPupilToPupilDtoMapper>()
            .AddSingleton<IMapper<MappableLearner, Pupil>, MapMappableLearnerToPupilMapper>();

        return services;
    }

    private static IServiceCollection AddMyPupilsInfrastructure(this IServiceCollection services)
    {
        services
            .AddScoped<IUserReadOnlyRepository, CosmosDbUserReadOnlyRepository>()
            .AddScoped<IUserAggregateWriteRepository, CosmosDbUserAggregateWriteRepository>()
            .AddSingleton<IMapper<UserDto, User.Application.Repository.UserReadRepository.User>, MapUserProfileDtoToUserMapper>();

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
