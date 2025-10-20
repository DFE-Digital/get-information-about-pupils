using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Downloads.Application.DatasetCheckers;
using DfE.GIAP.Core.Downloads.Application.Repositories;
using DfE.GIAP.Core.Downloads.Application.UseCases.GetAvailableDatasetsForPupils;
using DfE.GIAP.Core.Downloads.Infrastructure.Repositories;
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
            .RegisterApplicationDatasetCheckers()
            .RegisteApplicationUseCases();
    }


    private static IServiceCollection RegisteApplicationUseCases(this IServiceCollection services)
    {
        services.AddScoped<IUseCase<GetAvailableDatasetsForPupilsRequest, GetAvailableDatasetsForPupilsResponse>, GetAvailableDatasetsForPupilsUseCase>();

        return services;
    }

    private static IServiceCollection RegisterApplicationDatasetCheckers(this IServiceCollection services)
    {
        services.AddScoped<IDatasetAvailabilityChecker, FurtherEducationDatasetChecker>();
        services.AddScoped<IDatasetAvailabilityCheckerFactory, DatasetAvailabilityCheckerFactory>();

        return services;
    }

    // Infrastructure
    private static IServiceCollection RegisteInfrastructureDependencies(this IServiceCollection services)
    {
        return services
            .RegisterInfrastructureRepositories();
    }

    private static IServiceCollection RegisterInfrastructureRepositories(this IServiceCollection services)
    {
        services.AddScoped<IFurtherEducationRepository, CosmosFurtherEducationRepository>();

        return services;
    }
}
