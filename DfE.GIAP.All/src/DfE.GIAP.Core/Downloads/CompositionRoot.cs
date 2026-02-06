using System.Collections.Generic;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.Infrastructure.CosmosDb.DataTransferObjects;
using DfE.GIAP.Core.Downloads.Application.Aggregators;
using DfE.GIAP.Core.Downloads.Application.Aggregators.Handlers;
using DfE.GIAP.Core.Downloads.Application.Aggregators.Handlers.Mappers;
using DfE.GIAP.Core.Downloads.Application.Ctf;
using DfE.GIAP.Core.Downloads.Application.Ctf.Builders;
using DfE.GIAP.Core.Downloads.Application.Datasets.Access.Policies;
using DfE.GIAP.Core.Downloads.Application.Datasets.Access.Rules;
using DfE.GIAP.Core.Downloads.Application.Datasets.Availability;
using DfE.GIAP.Core.Downloads.Application.Datasets.Availability.Handlers;
using DfE.GIAP.Core.Downloads.Application.Enums;
using DfE.GIAP.Core.Downloads.Application.FileExports;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.DownloadOutputs;
using DfE.GIAP.Core.Downloads.Application.Repositories;
using DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilCtf;
using DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilDatasets;
using DfE.GIAP.Core.Downloads.Application.UseCases.GetAvailableDatasetsForPupils;
using DfE.GIAP.Core.Downloads.Infrastructure.FileExports;
using DfE.GIAP.Core.Downloads.Infrastructure.Repositories;
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

        // WIP
        services.AddScoped<IUseCase<DownloadPupilCtfRequest, DownloadPupilCtfResponse>, DownloadPupilCtfUseCase>();
        services.AddScoped<ICtfHeaderBuilder, CtfHeaderBuilder>();
        services.AddScoped<ICtfPupilBuilder, SingleFileCtfPupilBuilder>();
        services.AddScoped<ICtfFormatter, XmlCtfFormatter>();

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
        services.AddScoped<IMapper<PupilPremiumPupil, IEnumerable<PupilPremiumOutputRecord>>, PupilPremiumPupilToPupilPremiumOutputRecordMapper>();
        services.AddScoped<IMapper<FurtherEducationPupil, IEnumerable<FurtherEducationPPOutputRecord>>, FurtherEducationPupilToPpOutputRecordMapper>();
        services.AddScoped<IMapper<FurtherEducationPupil, IEnumerable<FurtherEducationSENOutputRecord>>, FurtherEducationPupilToSenOutputRecordMapper>();
        services.AddScoped<IMapper<NationalPupil, IEnumerable<CensusAutumnOutputRecord>>, NationalPupilToCensusAutumnOutputRecordMapper>();
        services.AddScoped<IMapper<NationalPupil, IEnumerable<CensusSummerOutputRecord>>, NationalPupilToCensusSummerOutputRecordMapper>();
        services.AddScoped<IMapper<NationalPupil, IEnumerable<CensusSpringOutputRecord>>, NationalPupilToCensusSpringOutputRecordMapper>();
        services.AddScoped<IMapper<NationalPupil, IEnumerable<EYFSPOutputRecord>>, NationalPupilToEyfspOutputRecordMapper>();
        services.AddScoped<IMapper<NationalPupil, IEnumerable<KS1OutputRecord>>, NationalPupilToKs1OutputRecordMapper>();
        services.AddScoped<IMapper<NationalPupil, IEnumerable<KS2OutputRecord>>, NationalPupilToKs2OutputRecordMapper>();
        services.AddScoped<IMapper<NationalPupil, IEnumerable<KS4OutputRecord>>, NationalPupilToKs4OutputRecordMapper>();
        services.AddScoped<IMapper<NationalPupil, IEnumerable<MTCOutputRecord>>, NationalPupilToMtcOutputRecordMapper>();
        services.AddScoped<IMapper<NationalPupil, IEnumerable<PhonicsOutputRecord>>, NationalPupilToPhonicsOutputRecordMapper>();

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
