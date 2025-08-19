using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.Infrastructure;
using DfE.GIAP.Core.PrePreparedDownloads.Application.FolderPath;
using DfE.GIAP.Core.PrePreparedDownloads.Application.UseCases.DownloadPrePreparedFile;
using Microsoft.Extensions.DependencyInjection;

namespace DfE.GIAP.Core.PrePreparedDownloads;
public static class CompositionRoot
{
    public static IServiceCollection AddPrePreparedDownloadsDependencies(this IServiceCollection services)
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
            .RegisterApplicationUseCases()
            .RegisterApplicationPathContext();
    }

    private static IServiceCollection RegisterApplicationUseCases(this IServiceCollection services)
    {
        return services
            .AddScoped<IUseCase<DownloadPrePreparedFileRequest, DownloadPrePreparedFileResponse>, DownloadPrePreparedFileUseCase>();
    }

    private static IServiceCollection RegisterApplicationPathContext(this IServiceCollection services)
    {
        return services
            .AddScoped<IBlobStoragePathResolver, AzureBlobStoragePathResolver>();
    }

    // Infrastructure 
    private static IServiceCollection RegisterInfrastructureDependencies(this IServiceCollection services)
    {
        return services
            .AddBlobStorageProvider()
            .RegisterInfrastructureRepositories()
            .RegisterInfrastructureMappers();
    }

    private static IServiceCollection RegisterInfrastructureRepositories(this IServiceCollection services)
    {
        return services;
    }

    private static IServiceCollection RegisterInfrastructureMappers(this IServiceCollection services)
    {
        return services;
    }
}
