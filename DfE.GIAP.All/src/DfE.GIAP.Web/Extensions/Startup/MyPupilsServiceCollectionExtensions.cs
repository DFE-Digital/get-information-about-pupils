using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils;
using DfE.GIAP.Web.Features.MyPupils.GetPaginatedMyPupils.PresentationHandlers.Order;
using DfE.GIAP.Web.Features.MyPupils.GetPaginatedMyPupils.PresentationHandlers.Paginate;
using DfE.GIAP.Web.Features.MyPupils;
using DfE.GIAP.Web.Session.Abstraction;
using DfE.GIAP.Web.Features.MyPupils.State.Selection;
using DfE.GIAP.Web.Session.Infrastructure.Serialization;
using DfE.GIAP.Web.Features.MyPupils.State.Selection.DataTransferObjects;
using DfE.GIAP.Web.Features.MyPupils.State.Selection.Mapper;
using Microsoft.Extensions.DependencyInjection;
using DfE.GIAP.Web.Features.MyPupils.GetPaginatedMyPupils;
using DfE.GIAP.Web.Features.MyPupils.GetPaginatedMyPupils.PresentationHandlers;
using DfE.GIAP.Web.Features.MyPupils.GetMyPupilsForUser.Mapper;
using DfE.GIAP.Web.Features.MyPupils.GetMyPupilsForUser.ViewModel;
using DfE.GIAP.Web.Features.MyPupils.State.Presentation;
using DfE.GIAP.Web.Features.MyPupils.UpdateMyPupilsState.Mapper;
using DfE.GIAP.Web.Features.MyPupils.GetSelectedMyPupils;
using DfE.GIAP.Web.Features.MyPupils.UpdateMyPupilsState;
using DfE.GIAP.Web.Features.MyPupils.UpdateMyPupilsState.PupilSelectionStateUpdater;
using DfE.GIAP.Web.Session.Abstraction.Query;
using DfE.GIAP.Web.Session.Infrastructure.AspNetCore.Query;
using DfE.GIAP.Web.Session.Abstraction.Command;
using DfE.GIAP.Web.Session.Infrastructure.AspNetCore.Command;
using DfE.GIAP.Web.Features.MyPupils.GetMyPupilsForUser;
using DfE.GIAP.Web.Features.MyPupils.State;
using DfE.GIAP.Web.Session.Infrastructure.AspNetCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DfE.GIAP.Web.Extensions.Startup;

public static class MyPupilsServiceCollectionExtensions
{
    public static IServiceCollection AddMyPupils(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddMyPupilsDependencies()
            .AddSessionStateHandlers()
            .AddGetPaginatedMyPupils()
            .AddGetMyPupils()
            .AddUpdateMyPupilsState()
            .AddGetSelectedMyPupils();

        return services;
    }

    private static IServiceCollection AddGetSelectedMyPupils(this IServiceCollection services)
    {
        services.AddScoped<IGetSelectedMyPupilsProvider, GetSelectedMyPupilsProvider>();
        return services;
    }

    private static IServiceCollection AddUpdateMyPupilsState(this IServiceCollection services)
    {
        services
            .AddSingleton<IMapper<MyPupilsFormStateRequestDto, MyPupilsPresentationState>, MapFormStateRequestDtoToMyPupilsPresentationStateMapper>() // TODO needs tests
            .AddSingleton<IPupilSelectionStateUpdateHandler, PupilSelectionStateUpdateHandler>()
            .AddScoped<IUpdateMyPupilsStateHandler, UpdateMyPupilsStateHandler>();
        return services;
    }

    private static IServiceCollection AddGetMyPupils(this IServiceCollection services)
    {
        services
            .AddSingleton<IMapper<MyPupilsDtoSelectionStateDecorator, PupilsViewModel>, MyPupilDtoPupilSelectionStateDecoratorToPupilsViewModelMapper>()
            .AddScoped<IGetMyPupilsForUserHandler, GetMyPupilsForUserHandler>();
        return services;
    }

    private static IServiceCollection AddGetPaginatedMyPupils(this IServiceCollection services)
    {
        // TODO implement the generic ChainedEvaluationHandler and IEvaluationHandler<Tin, TOut>
        services.AddSingleton<OrderMyPupilDtosPresentationHandler>();
        services.AddSingleton<PaginateMyPupilDtosPresentationHandler>();

        services.AddSingleton<IMyPupilDtosPresentationHandler>(sp =>
        {
            return new ChainedEvaluationMyPupilDtosPresentationHandler(
                    current: sp.GetRequiredService<OrderMyPupilDtosPresentationHandler>())
                .ChainNext(next: sp.GetRequiredService<PaginateMyPupilDtosPresentationHandler>());
        });
        services.AddScoped<IGetPaginatedMyPupilsHandler, GetPaginatedMyPupilsHandler>();

        return services;
    }

    private static IServiceCollection AddSessionStateHandlers(this IServiceCollection services)
    {
        services
            .AddScoped<ISessionQueryHandler<MyPupilsPresentationState>, AspNetCoreSessionQueryHandler<MyPupilsPresentationState>>()
            .AddScoped<ISessionCommandHandler<MyPupilsPresentationState>, AspNetCoreSessionCommandHandler<MyPupilsPresentationState>>()

            .AddScoped<ISessionQueryHandler<MyPupilsPupilSelectionState>, AspNetCoreSessionQueryHandler<MyPupilsPupilSelectionState>>()
            .AddScoped<ISessionCommandHandler<MyPupilsPupilSelectionState>, AspNetCoreSessionCommandHandler<MyPupilsPupilSelectionState>>()
            .AddScoped<IGetMyPupilsStateProvider, GetMyPupilsStateProvider>()
            // Serailizers into State
            .AddSingleton<MyPupilsPupilSelectionStateFromDtoMapper>()
            .AddSingleton<MyPupilsPupilSelectionStateToDtoMapper>()
            .AddScoped<ISessionObjectSerializer<MyPupilsPupilSelectionState>>(sp =>
            {
                return new MappedToDataTransferObjectSessionObjectSerializer<MyPupilsPupilSelectionState, MyPupilsPupilSelectionStateDto>(
                    sp.GetRequiredService<MyPupilsPupilSelectionStateToDtoMapper>(),
                    sp.GetRequiredService<MyPupilsPupilSelectionStateFromDtoMapper>());
            })
            .AddScoped<ISessionObjectSerializer<MyPupilsPresentationState>, DefaultSessionObjectSerializer<MyPupilsPresentationState>>();
        return services;
    }
}
