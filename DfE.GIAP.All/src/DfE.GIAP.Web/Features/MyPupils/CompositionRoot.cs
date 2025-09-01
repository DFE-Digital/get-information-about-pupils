using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils;
using DfE.GIAP.Web.Features.MyPupils.Handlers.GetMyPupils;
using DfE.GIAP.Web.Features.MyPupils.Handlers.GetMyPupils.Mapper;
using DfE.GIAP.Web.Features.MyPupils.Handlers.GetPaginatedMyPupils;
using DfE.GIAP.Web.Features.MyPupils.Handlers.GetPaginatedMyPupils.PresentationHandlers;
using DfE.GIAP.Web.Features.MyPupils.Handlers.GetPaginatedMyPupils.PresentationHandlers.Order;
using DfE.GIAP.Web.Features.MyPupils.Handlers.GetPaginatedMyPupils.PresentationHandlers.Paginate;
using DfE.GIAP.Web.Features.MyPupils.Handlers.UpdateMyPupilsState;
using DfE.GIAP.Web.Features.MyPupils.SelectionState;
using DfE.GIAP.Web.Features.MyPupils.SelectionState.Provider.DataTransferObjects;
using DfE.GIAP.Web.Features.MyPupils.SelectionState.Provider.Mapper;
using DfE.GIAP.Web.Features.MyPupils.ViewModels;
using DfE.GIAP.Web.Features.Session.Abstractions;
using DfE.GIAP.Web.Features.Session.Infrastructure.Serialization;

namespace DfE.GIAP.Web.Features.MyPupils;

public static class CompositionRoot
{

    public static IServiceCollection AddMyPupils(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services
            .AddMyPupilsApplication()
            .AddMyPupilsPresentation();

        return services;
    }

    private static IServiceCollection AddMyPupilsPresentation(this IServiceCollection services)
    {
        // GetPaginatedMyPupilsHandler
        services.AddScoped<IGetPaginatedMyPupilsHandler, GetPaginatedMyPupilsHandler>();
        services.AddSingleton<OrderPupilDtosPresentationHandler>();
        services.AddSingleton<PaginatePupilDtosPresentationHandler>();
        services.AddSingleton<IPupilDtosPresentationHandler>(sp =>
        {
            return
                new ChainedEvaluationPupilDtosPresentationHandler(sp.GetRequiredService<OrderPupilDtosPresentationHandler>())
                    .ChainNext(sp.GetRequiredService<PaginatePupilDtosPresentationHandler>());
        });

        // GetMyPupilsHandler
        services.AddScoped<IGetMyPupilsHandler, GetMyPupilsHandler>();
        services.AddSingleton<IMapper<PupilDtoWithSelectionStateDecorator, PupilViewModel>, PupilDtoWithSelectionStateToPupilPresentationViewModelMapper>();

        // UpdateMyPupilsStateHandler
        services.AddScoped<IUpdateMyPupilsStateHandler, UpdateMyPupilsStateHandler>();

        // MyPupilsPupilSelectionState
        services.AddSingleton<IMapper<MyPupilsPupilSelectionState, MyPupilsPupilSelectionStateDto>, MyPupilsPupilSelectionStateToDtoMapper>();
        services.AddSingleton<IMapper<MyPupilsPupilSelectionStateDto, MyPupilsPupilSelectionState>, MyPupilsPupilSelectionStateFromDtoMapper>();

        // Session
        services.AddScoped<ISessionObjectSerializer<MyPupilsPupilSelectionState>>(sp =>
        {
            return new MappedToDataTransferObjectSessionObjectSerializer<MyPupilsPupilSelectionState, MyPupilsPupilSelectionStateDto>
            (
                sp.GetRequiredService<IMapper<MyPupilsPupilSelectionState, MyPupilsPupilSelectionStateDto>>(),
                sp.GetRequiredService<IMapper<MyPupilsPupilSelectionStateDto, MyPupilsPupilSelectionState>>());
        });

        return services;
    }

    private static IServiceCollection AddMyPupilsApplication(this IServiceCollection services)
    {
        services.AddMyPupilsDependencies();
        return services;
    }
}
