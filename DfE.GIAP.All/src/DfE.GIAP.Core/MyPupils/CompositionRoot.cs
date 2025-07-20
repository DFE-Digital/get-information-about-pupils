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
            .AddScoped<IAggregatePupilsForMyPupilsDomainService, AggregatePupilsForMyPupilsDomainService>()
            .AddSingleton<IUserAggregateRootFactory, UserAggregateRootFactory>();

        return services;
    }

    private static IServiceCollection AddMyPupilsApplication(this IServiceCollection services)
    {
        services
            .AddScoped<IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>, GetMyPupilsUseCase>()
            .AddSingleton<IMapper<IAuthorisationContext, PupilAuthorisationContext>, MapAuthorisationContextToPupilsAuthorisationContextMapper>();

        return services;
    }

    private static IServiceCollection AddMyPupilsInfrastructure(this IServiceCollection services)
    {
        services
            .AddScoped<IUserReadOnlyRepository, CosmosDbUserReadOnlyRepository>()
            .AddSingleton<IMapper<UserProfileDto, User.Application.Repository.User>, MapUserProfileDtoToUserMapper>();
        return services;
    }
}
