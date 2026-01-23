using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Downloads.Application.Aggregators;
using DfE.GIAP.Core.Downloads.Application.Aggregators.Handlers;
using DfE.GIAP.Core.Downloads.Application.Aggregators.Handlers.Mappers;
using DfE.GIAP.Core.Downloads.Application.Datasets.Access.Policies;
using DfE.GIAP.Core.Downloads.Application.Datasets.Access.Rules;
using DfE.GIAP.Core.Downloads.Application.Datasets.Availability;
using DfE.GIAP.Core.Downloads.Application.Datasets.Availability.Handlers;
using DfE.GIAP.Core.Downloads.Application.Enums;
using DfE.GIAP.Core.Downloads.Application.FileExports;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.DownloadOutputs;
using DfE.GIAP.Core.Downloads.Application.Repositories;
using DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilDatasets;
using DfE.GIAP.Core.Downloads.Application.UseCases.GetAvailableDatasetsForPupils;
using DfE.GIAP.Core.Downloads.Infrastructure.FileExports;
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
            .RegisterApplicationAggregatorsAndHandlers()
            .RegisterApplicationMappers()
            .RegisteApplicationUseCases();
    }


    private static IServiceCollection RegisteApplicationUseCases(this IServiceCollection services)
    {
        services.AddScoped<IUseCase<GetAvailableDatasetsForPupilsRequest, GetAvailableDatasetsForPupilsResponse>, GetAvailableDatasetsForPupilsUseCase>();
        services.AddScoped<IUseCase<DownloadPupilDataRequest, DownloadPupilDataResponse>, DownloadPupilDataUseCase>();

        return services;
    }

    private static IServiceCollection RegisterApplicationAggregatorsAndHandlers(this IServiceCollection services)
    {
        services.AddScoped<IPupilDatasetAggregatorFactory, PupilDatasetAggregationHandlerFactory>();
        services.AddScoped<IPupilDatasetAggregationHandler, FurtherEducationAggregationHandler>();
        services.AddScoped<IPupilDatasetAggregationHandler, NationalPupilDatabaseAggregationHandler>();
        services.AddScoped<IPupilDatasetAggregationHandler, PupilPremiumAggregationHandler>();

        return services;

    }

    private static IServiceCollection RegisterApplicationMappers(this IServiceCollection services)
    {
        services.AddScoped<IMapper<PupilPremiumPupil, PupilPremiumOutputRecord>, PupilPremiumPupilToPupilPremiumOutputRecordMapper>();
        services.AddScoped<IMapper<FurtherEducationPupil, FurtherEducationPPOutputRecord>, FurtherEducationPupilToPpOutputRecordMapper>();
        services.AddScoped<IMapper<FurtherEducationPupil, FurtherEducationSENOutputRecord>, FurtherEducationPupilToSenOutputRecordMapper>();
        services.AddScoped<IMapper<NationalPupil, CensusAutumnOutput>, NationalPupilToCensusAutumnOutputRecordMapper>();
        services.AddScoped<IMapper<NationalPupil, CensusSummerOutput>, NationalPupilToCensusSummerOutputRecordMapper>();
        services.AddScoped<IMapper<NationalPupil, CensusSpringOutput>, NationalPupilToCensusSpringOutputRecordMapper>();
        services.AddScoped<IMapper<NationalPupil, EYFSPOutput>, NationalPupilToEyfspOutputRecordMapper>();
        services.AddScoped<IMapper<NationalPupil, KS1Output>, NationalPupilToKs1OutputRecordMapper>();
        services.AddScoped<IMapper<NationalPupil, KS2Output>, NationalPupilToKs2OutputRecordMapper>();
        services.AddScoped<IMapper<NationalPupil, KS4Output>, NationalPupilToKs4OutputRecordMapper>();
        services.AddScoped<IMapper<NationalPupil, MTCOutput>, NationalPupilToMtcOutputRecordMapper>();
        services.AddScoped<IMapper<NationalPupil, PhonicsOutput>, NationalPupilToPhonicsOutputRecordMapper>();

        return services;
    }

    private static IServiceCollection RegisterApplicationDatasetEvaluatorsAndHandlers(this IServiceCollection services)
    {
        services.AddScoped<IDatasetAvailabilityHandlerFactory, DatasetAvailabilityHandlerFactory>();
        services.AddScoped<IDatasetAvailabilityHandler, FurtherEducationDatasetHandler>();
        services.AddScoped<IDatasetAvailabilityHandler, NationalPupilDatasetHandler>();
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
                [Dataset.Census_Autumn] = DatasetAccessPolicies.CensusAutumn(),
                [Dataset.Census_Spring] = DatasetAccessPolicies.CensusSpring(),
                [Dataset.Census_Summer] = DatasetAccessPolicies.CensusSummer(),
                [Dataset.PP] = DatasetAccessPolicies.PupilPremium(),
                [Dataset.SEN] = DatasetAccessPolicies.SpecialEducationNeeds(),
                [Dataset.FE_PP] = DatasetAccessPolicies.FEPupilPremium()
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
            .RegisterInfrastructureMappers()
            .RegisterInfrastructureFileExports();
    }

    private static IServiceCollection RegisterInfrastructureRepositories(this IServiceCollection services)
    {
        services.AddScoped<IFurtherEducationReadOnlyRepository, CosmosDbFurtherEducationReadOnlyRepository>();
        services.AddScoped<INationalPupilReadOnlyRepository, CosmosDbNationalPupilReadOnlyRepository>();
        services.AddScoped<IPupilPremiumReadOnlyRepository, CosmosDbPupilPremiumReadOnlyRepository>();

        return services;
    }

    private static IServiceCollection RegisterInfrastructureMappers(this IServiceCollection services)
    {
        services.AddScoped<IMapper<FurtherEducationPupilDto, FurtherEducationPupil>, FurtherEducationPupilDtoToEntityMapper>();
        services.AddScoped<IMapper<NationalPupilDto, NationalPupil>, NationalPupilDtoToEntityMapper>();
        services.AddScoped<IMapper<PupilPremiumPupilDto, PupilPremiumPupil>, PupilPremiumDtoToEntityMapper>();

        return services;
    }

    private static IServiceCollection RegisterInfrastructureFileExports(this IServiceCollection services)
    {
        services.AddScoped<IDelimitedFileExporter, DelimitedFileExporter>();
        services.AddScoped<IZipArchiveBuilder, ZipArchiveBuilder>();
        return services;
    }
}
