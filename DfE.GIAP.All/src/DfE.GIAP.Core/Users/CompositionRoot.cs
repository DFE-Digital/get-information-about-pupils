using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Users.Application;
using DfE.GIAP.Core.Users.Application.Repository;
using DfE.GIAP.Core.Users.Infrastructure.Repository;
using DfE.GIAP.Core.Users.Infrastructure.Repository.Dtos;
using Microsoft.Extensions.DependencyInjection;

namespace DfE.GIAP.Core.Users;
public static class CompositionRoot
{
    public static IServiceCollection AddUserDependencies(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services
            .RegisterApplicationDependencies()
            .RegisterInfrastructureDependencies();
    }

    // Application
    private static IServiceCollection RegisterApplicationDependencies(this IServiceCollection services)
    {
        return services;
    }

    // Infrastructure
    private static IServiceCollection RegisterInfrastructureDependencies(this IServiceCollection services)
    {
        return services
            .RegisterInfrastructureRepositories()
            .RegisterInfrastructureMappers();
    }

    private static IServiceCollection RegisterInfrastructureRepositories(this IServiceCollection services)
    {
        return services
            .AddScoped<IUserReadOnlyRepository, CosmosDbUserReadOnlyRepository>();
    }

    private static IServiceCollection RegisterInfrastructureMappers(this IServiceCollection services)
    {
        return services
            .AddScoped<IMapper<UserDto, User>, UserProfileDtoToUserMapper>();
    }
}
