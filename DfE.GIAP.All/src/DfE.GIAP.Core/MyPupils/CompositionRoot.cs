using DfE.GIAP.Core.Common.Application.Mappers;
using DfE.GIAP.Core.Common.Application.ValueObjects;
using DfE.GIAP.Core.MyPupils.Application.Options;
using DfE.GIAP.Core.MyPupils.Application.Ports;
using DfE.GIAP.Core.MyPupils.Application.Repositories;
using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils;
using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils.Handlers;
using DfE.GIAP.Core.MyPupils.Application.UseCases.AddPupilsToMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Infrastructure.Adaptors;
using DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.Read;
using DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.Write;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.Models;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
            .AddSingleton<IMapper<Pupil, MyPupilsModel>, PupilToMyPupilsModelMapper>()

            .AddScoped<IUseCaseRequestOnly<AddPupilsToMyPupilsRequest>, AddPupilsToMyPupilsUseCase>()
            .AddScoped<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>, DeletePupilsFromMyPupilsUseCase>()
            // AggregatePupilsService
            .AddScoped<IAggregatePupilsForMyPupilsApplicationService, AggregatePupilsForMyPupilsApplicationService>()
            .AddSingleton<IOrderPupilsHandler, OrderPupilsHandler>()
            .AddSingleton<IPaginatePupilsHandler, PaginatePupilsHandler>();

        return services;
    }

    private static IServiceCollection AddMyPupilsInfrastructure(this IServiceCollection services)
    {
        services
            .AddScoped<IMyPupilsReadOnlyRepository, CosmosDbMyPupilsReadOnlyRepository>()
            .AddScoped<IMyPupilsWriteOnlyRepository, CosmosDbMyPupilsWriteOnlyRepository>()
            .AddSingleton<IMapper<MyPupilsAggregate, MyPupilsDocumentDto>, MyPupilsAggregateToMyPupilsDocumentDtoMapper>();
            
        services.TryAddScoped<IQueryMyPupilsPort, QueryMyPupilsSearchAdaptor>();
        services.TryAddSingleton<IMapper<NationalPupilDatabaseLearner, Pupil>, NationalPupilDatabaseLearnerToPupilMapper>();
        services.TryAddSingleton<IMapper<PupilPremiumLearner, Pupil>, PupilPremiumLearnerToPupilMapper>();
        return services;
    }
}
