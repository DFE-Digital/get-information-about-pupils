using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.Infrastructure;
using DfE.GIAP.Core.PreparedDownloads.Application.FolderPath;
using DfE.GIAP.Core.PreparedDownloads.Application.UseCases.DownloadPreparedFile;
using DfE.GIAP.Core.PreparedDownloads.Application.UseCases.GetPreparedFiles;
using Microsoft.Extensions.DependencyInjection;

namespace DfE.GIAP.Core.PreparedDownloads;
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
            .RegisterApplicationUseCases();
    }

    private static IServiceCollection RegisterApplicationUseCases(this IServiceCollection services)
    {
        return services
            .AddScoped<IUseCase<DownloadPreparedFileRequest, DownloadPreparedFileResponse>, DownloadPreparedFileUseCase>()
            .AddScoped<IUseCase<GetPreparedFilesRequest, GetPreparedFilesResponse>, GetPreparedFilesUseCase>();
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
