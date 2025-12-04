using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils;
using DfE.GIAP.Core.MyPupils.Application.Options;
using DfE.GIAP.Web.Features.MyPupils.Services.GetMyPupilsForUser;
using DfE.GIAP.Web.Features.MyPupils.Services.GetMyPupilsForUser.Mapper;
using DfE.GIAP.Web.Features.MyPupils.Services.GetMyPupilsForUser.ViewModels;
using DfE.GIAP.Web.Features.MyPupils.Services.GetPaginatedMyPupils.PresentationHandlers;
using DfE.GIAP.Web.Features.MyPupils.Services.GetPaginatedMyPupils.PresentationHandlers.Order;
using DfE.GIAP.Web.Features.MyPupils.Services.GetPaginatedMyPupils.PresentationHandlers.Paginate;
using DfE.GIAP.Web.Features.MyPupils.Services.GetPupilViewModels.GetSelectedMyPupils;
using DfE.GIAP.Web.Features.MyPupils.State;
using DfE.GIAP.Web.Features.MyPupils.State.Presentation;
using DfE.GIAP.Web.Features.MyPupils.State.Selection;
using DfE.GIAP.Web.Features.MyPupils.State.Selection.DataTransferObjects;
using DfE.GIAP.Web.Features.MyPupils.State.Selection.Mapper;
using DfE.GIAP.Web.Features.MyPupils.ViewModels.Factory;
using DfE.GIAP.Web.Session.Abstraction;
using DfE.GIAP.Web.Session.Abstraction.Command;
using DfE.GIAP.Web.Session.Abstraction.Query;
using DfE.GIAP.Web.Session.Infrastructure.AspNetCore.Command;
using DfE.GIAP.Web.Session.Infrastructure.AspNetCore.Query;
using DfE.GIAP.Web.Session.Infrastructure.Serialization;

namespace DfE.GIAP.Web.Features.MyPupils;

public static class CompositionRoot
{
    public static IServiceCollection AddMyPupils(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddMyPupilsDependencies()
            .AddSingleton<IMyPupilsViewModelFactory, MyPupilsViewModelFactory>()
            .AddOptions<MyPupilsOptions>();

        services
            .AddSessionStateHandlers()
            .AddGetPupilViewModels()
            .AddGetSelectedMyPupils();

        return services;
    }

    private static IServiceCollection AddGetSelectedMyPupils(this IServiceCollection services)
    {
        services.AddScoped<IGetSelectedMyPupilsHandler, GetSelectedMyPupilsHandler>();
        return services;
    }

    private static IServiceCollection AddGetPupilViewModels(this IServiceCollection services)
    {
        services
            .AddSingleton<IMapper<PupilsSelectionContext, PupilsViewModel>, PupilsSelectionContextToPupilsViewModelMapper
            >()
            .AddScoped<IGetPupilViewModelsHandler, GetPupilViewModelsHandler>();
        // TODO implement the generic ChainedEvaluationHandler and IEvaluationHandler<Tin, TOut>
        services.AddSingleton<OrderMyPupilDtosPresentationHandler>();
        services.AddSingleton<PaginateMyPupilDtosPresentationHandler>();

        services.AddSingleton<IMyPupilDtosPresentationHandler>(sp =>
        {
            return new ChainedEvaluationMyPupilDtosPresentationHandler(
                    current: sp.GetRequiredService<OrderMyPupilDtosPresentationHandler>())
                .ChainNext(next: sp.GetRequiredService<PaginateMyPupilDtosPresentationHandler>());
        });

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
            .AddSingleton<ISessionObjectSerializer<MyPupilsPupilSelectionState>>(sp =>
            {
                return new MappedToDataTransferObjectSessionObjectSerializer<MyPupilsPupilSelectionState, MyPupilsPupilSelectionStateDto>(
                    sp.GetRequiredService<MyPupilsPupilSelectionStateToDtoMapper>(),
                    sp.GetRequiredService<MyPupilsPupilSelectionStateFromDtoMapper>());
            })
            .AddSingleton<ISessionObjectSerializer<MyPupilsPresentationState>, DefaultSessionObjectSerializer<MyPupilsPresentationState>>();
        return services;
    }
}
