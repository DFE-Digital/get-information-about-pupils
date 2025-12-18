using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.MyPupils.Application.Mapper;
using DfE.GIAP.Core.MyPupils.Application.Options;
using DfE.GIAP.Core.MyPupils.Application.Repositories;
using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils;
using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils.DataTransferObjects;
using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils.Mapper;
using DfE.GIAP.Core.MyPupils.Application.UseCases.AddPupilsToMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.DeleteAllPupilsFromMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.Read;
using DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.Write;
using DfE.GIAP.Core.MyPupils.Infrastructure.Search;
using Microsoft.Extensions.DependencyInjection;

namespace DfE.GIAP.Core.MyPupils;
public static class CompositionRoot
{
    public static IServiceCollection AddMyPupilsCore(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services
            .AddOptions<MyPupilsOptions>();

        services
            .AddMyPupilsApplication()
            .AddMyPupilsInfrastructure();

        return services;
    }

    private static IServiceCollection AddMyPupilsApplication(this IServiceCollection services)
    {
        services
            .AddSingleton<IMapper<IEnumerable<string>, UniquePupilNumbers>, UniquePupilNumbersMapper>()
            // UseCases
            .AddScoped<IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>, GetMyPupilsUseCase>()
            .AddSingleton<IMapper<Pupil, MyPupilsModel>, PupilToMyPupilModelMapper>()

            .AddScoped<IUseCaseRequestOnly<AddPupilsToMyPupilsRequest>, AddPupilsToMyPupilsUseCase>()
            .AddScoped<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>, DeletePupilsFromMyPupilsUseCase>()
            .AddScoped<IUseCaseRequestOnly<DeleteAllMyPupilsRequest>, DeleteAllMyPupilsUseCase>()
            // AggregatePupilsService
            .AddScoped<IAggregatePupilsForMyPupilsApplicationService, AggregatePupilsForMyPupilsApplicationService>()
            .AddSingleton<IMapper<AzureIndexEntityWithPupilType, Pupil>, AzureIndexEntityWithPupilTypeToPupilMapper>();

        return services;
    }

    private static IServiceCollection AddMyPupilsInfrastructure(this IServiceCollection services)
    {
        services
            .AddScoped<IMyPupilsReadOnlyRepository, CosmosDbMyPupilsReadOnlyRepository>()
            .AddScoped<IMyPupilsWriteOnlyRepository, CosmosDbMyPupilsWriteOnlyRepository>()
            .AddSingleton<IMapper<MyPupilsAggregate, MyPupilsDocumentDto>, MyPupilsAggregateToMyPupilsDocumentDtoMapper>()
            // Temporary SearchClients and SearchClientProvider
            // Note: depends on the infrastructure.cognitivesearch packages being registered
            .AddSearchClients();

        return services;
    }
}
