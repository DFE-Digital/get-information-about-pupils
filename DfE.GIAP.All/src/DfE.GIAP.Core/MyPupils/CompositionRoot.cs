using Azure.Search.Documents;
using Azure;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.AuthorisationContext;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Services;
using DfE.GIAP.Core.MyPupils.Domain.Aggregate;
using DfE.GIAP.Core.MyPupils.Domain.Authorisation;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Domain.Services;
using DfE.GIAP.Core.User.Application.Repository;
using DfE.GIAP.Core.User.Infrastructure.Repository;
using Microsoft.Extensions.DependencyInjection;
using DfE.GIAP.Core.MyPupils.Application.Options;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using DfE.GIAP.Core.MyPupils.Application.Options.Extensions;

namespace DfE.GIAP.Core.MyPupils;
public static class CompositionRoot
{
    public static IServiceCollection AddMyPupilsDependencies(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services
            .AddMyPupilsDomain()
            .AddMyPupilsApplication()
            .AddMyPupilsInfrastructure();

        return services;
    }

    private static IServiceCollection AddMyPupilsDomain(this IServiceCollection services)
    {
        services
            .AddScoped<IAggregatePupilsForMyPupilsDomainService, TempAggregatePupilsForMyPupilsDomainService>()
            .AddSingleton<IUserAggregateRootFactory, UserAggregateRootFactory>();

        return services;
    }

    private static IServiceCollection AddMyPupilsApplication(this IServiceCollection services)
    {
        services
            .AddScoped<IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>, GetMyPupilsUseCase>()
            .AddSingleton<IMapper<Pupil, PupilItemPresentationModel>, MapPupilToPupilPresentationModel>()
            .AddSingleton<IMapper<IAuthorisationContext, PupilAuthorisationContext>, MapAuthorisationContextToPupilsAuthorisationContextMapper>();

        return services;
    }

    private static IServiceCollection AddMyPupilsInfrastructure(this IServiceCollection services)
    {
        services
            .AddScoped<IUserReadOnlyRepository, CosmosDbUserReadOnlyRepository>()
            .AddSingleton<IMapper<UserProfileDto, User.Application.Repository.User>, MapUserProfileDtoToUserMapper>();

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
            IndexOptions indexOptions = options.GetIndexOptionsByKey("npd");

            SearchClient searchClient = new(
                new Uri(options.Url),
                indexOptions.IndexName,
                new AzureKeyCredential(options.Key));
            return searchClient;
        });

        services.AddSingleton<SearchClient>(sp =>
        {
            SearchIndexOptions options = sp.GetRequiredService<IOptions<SearchIndexOptions>>().Value;
            IndexOptions indexOptions = options.GetIndexOptionsByKey("pupil-premium");

            SearchClient searchClient = new(
                new Uri(options.Url),
                indexOptions.IndexName,
                new AzureKeyCredential(options.Key));
            return searchClient;
        });

        return services;
    }
}
