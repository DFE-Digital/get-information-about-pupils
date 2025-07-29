using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.Services;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Request;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.User.Application.Repository;
using DfE.GIAP.Core.User.Infrastructure.Repository;
using DfE.GIAP.Core.User.Infrastructure.Repository.Dtos;
using Microsoft.Extensions.DependencyInjection;

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
            .AddSingleton<IMapper<Pupil, PupilDto>, MapPupilToPupilDtoMapper>()

        return services;
    }

    private static IServiceCollection AddMyPupilsInfrastructure(this IServiceCollection services)
    {
        services
            .AddScoped<IUserReadOnlyRepository, CosmosDbUserReadOnlyRepository>()
            .AddSingleton<IMapper<UserDto, User.Application.User>, MapUserProfileDtoToUserMapper>();

        return services;
    }
}
