using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Users.Application;
using DfE.GIAP.Core.Users.Application.Repositories;
using DfE.GIAP.Core.Users.Application.UseCases.CreateUserIfNotExists;
using DfE.GIAP.Core.Users.Application.UseCases.GetUnreadUserNews;
using DfE.GIAP.Core.Users.Application.UseCases.UpdateLastLogin;
using DfE.GIAP.Core.Users.Infrastructure.Repositories;
using DfE.GIAP.Core.Users.Infrastructure.Repositories.Dtos;
using DfE.GIAP.Core.Users.Infrastructure.Repositories.Mappers;
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
        return services
            .RegisterApplicationUseCases();
    }

    private static IServiceCollection RegisterApplicationUseCases(this IServiceCollection services)
    {
        return services
            .AddScoped<IUseCase<GetUnreadUserNewsRequest, GetUnreadUserNewsResponse>, GetUnreadUserNewsUseCase>()
            .AddScoped<IUseCaseRequestOnly<CreateUserIfNotExistsRequest>, CreateUserIfNotExistsUseCase>()
            .AddScoped<IUseCaseRequestOnly<UpdateLastLoggedInRequest>, UpdateLastLoggedInUseCase>();
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
            .AddScoped<IUserReadOnlyRepository, CosmosDbUserReadOnlyRepository>()
            .AddScoped<IUserWriteOnlyRepository, CosmosDbUserWriteOnlyRepository>();
    }

    private static IServiceCollection RegisterInfrastructureMappers(this IServiceCollection services)
    {
        return services
            .AddScoped<IMapper<UserDto, User>, UserDtoToUserMapper>()
            .AddScoped<IMapper<User, UserDto>, UserToUserDtoMapper>();
    }
}
