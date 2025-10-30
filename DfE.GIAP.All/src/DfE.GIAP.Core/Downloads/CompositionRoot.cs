using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Downloads.Application.Datasets.Access.Policies;
using DfE.GIAP.Core.Downloads.Application.Datasets.Access.Rules;
using DfE.GIAP.Core.Downloads.Application.Datasets.Availability;
using DfE.GIAP.Core.Downloads.Application.Datasets.Availability.Handlers;
using DfE.GIAP.Core.Downloads.Application.Enums;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Repositories;
using DfE.GIAP.Core.Downloads.Application.UseCases.GetAvailableDatasetsForPupils;
using DfE.GIAP.Core.Downloads.Infrastructure.Repositories;
using DfE.GIAP.Core.Downloads.Infrastructure.Repositories.DataTransferObjects;
using DfE.GIAP.Core.Downloads.Infrastructure.Repositories.Mappers;
using Microsoft.Extensions.DependencyInjection;

namespace DfE.GIAP.Core.Downloads;

public static class CompositionRoot
{
    public static IServiceCollection AddDownloadDependencies(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services
            .RegisteApplicationDependencies()
            .RegisteInfrastructureDependencies();
    }

    // Application
    private static IServiceCollection RegisteApplicationDependencies(this IServiceCollection services)
    {
        return services
            .RegisterApplicationDatasetEvaluatorsAndHandlers()
            .RegisteApplicationUseCases();
    }


    private static IServiceCollection RegisteApplicationUseCases(this IServiceCollection services)
    {
        services.AddScoped<IUseCase<GetAvailableDatasetsForPupilsRequest, GetAvailableDatasetsForPupilsResponse>, GetAvailableDatasetsForPupilsUseCase>();

        return services;
    }

    private static IServiceCollection RegisterApplicationDatasetEvaluatorsAndHandlers(this IServiceCollection services)
    {
        services.AddScoped<IDatasetAvailabilityHandlerFactory, DatasetAvailabilityHandlerFactory>();
        services.AddScoped<IDatasetAvailabilityHandler, FurtherEducationDatasetHandler>();
        services.AddScoped<IDatasetAvailabilityHandler, NationalPupilDatabaseDatasetHandler>();
        services.AddSingleton<IDatasetAccessEvaluator>(_ =>
        {
            Dictionary<Dataset, IDatasetAccessRule> policies = new()
            {
                [Dataset.EYFSP] = DatasetAccessPolicies.EYFSP(),
                [Dataset.KS1] = DatasetAccessPolicies.KS1(),
                [Dataset.KS2] = DatasetAccessPolicies.KS2(),
                [Dataset.KS4] = DatasetAccessPolicies.KS4(),
                [Dataset.Phonics] = DatasetAccessPolicies.Phonics(),
                [Dataset.MTC] = DatasetAccessPolicies.Mtc(),
                [Dataset.PP] = DatasetAccessPolicies.PupilPremium(),
                [Dataset.SEN] = DatasetAccessPolicies.SpecialEducationNeeds(),
                [Dataset.Census_Autumn] = DatasetAccessPolicies.CensusAutumn(),
                [Dataset.Census_Spring] = DatasetAccessPolicies.CensusSpring(),
                [Dataset.Census_Summer] = DatasetAccessPolicies.CensusSummer()
            };

            return new DatasetAccessPolicyEvaluator(policies);
        });

        return services;
    }

    // Infrastructure
    private static IServiceCollection RegisteInfrastructureDependencies(this IServiceCollection services)
    {
        return services
            .RegisterInfrastructureRepositories()
            .RegisterInfrastructureMappers();
    }

    private static IServiceCollection RegisterInfrastructureRepositories(this IServiceCollection services)
    {
        services.AddScoped<IFurtherEducationReadOnlyRepository, CosmosDbFurtherEducationReadOnlyRepository>();
        services.AddScoped<INationalPupilReadOnlyRepository, CosmosDbNationalPupilReadOnlyRepository>();

        return services;
    }

    private static IServiceCollection RegisterInfrastructureMappers(this IServiceCollection services)
    {
        services.AddScoped<IMapper<FurtherEducationPupilDto, FurtherEducationPupil>, FurtherEducationPupilDtoToEntityMapper>();
        services.AddScoped<IMapper<NationalPupilDto, NationalPupil>, NationalPupilDtoToEntityMapper>();

        return services;
    }
}
