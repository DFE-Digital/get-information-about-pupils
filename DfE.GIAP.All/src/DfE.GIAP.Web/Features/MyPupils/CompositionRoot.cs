using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.Web.Features.MyPupils.Logging;
using DfE.GIAP.Web.Features.MyPupils.PresentationService;
using DfE.GIAP.Web.Features.MyPupils.PresentationService.Mapper;
using DfE.GIAP.Web.Features.MyPupils.PresentationService.PresentationHandlers;
using DfE.GIAP.Web.Features.MyPupils.State;
using DfE.GIAP.Web.Features.MyPupils.State.Models.Presentation;
using DfE.GIAP.Web.Features.MyPupils.State.Models.Selection;
using DfE.GIAP.Web.Features.MyPupils.State.Models.Selection.DataTransferObjects;
using DfE.GIAP.Web.Features.MyPupils.State.Models.Selection.Mapper;
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

        services
            .AddMyPupilsCore();
           
        services
            .AddSessionStateHandlers()
            .AddGetMyPupilsPresentationServices();

        return services;
    }


    private static IServiceCollection AddGetMyPupilsPresentationServices(this IServiceCollection services)
    {

        services.AddOptions<MyPupilsLoggingOptions>();

        services
            .AddSingleton<IMapper<MyPupilsModel, MyPupilsPresentationPupilModels>, MyPupilModelsToMyPupilsPresentationPupilModelMapper>()
            .AddSingleton<IMapper<MyPupilModel, MyPupilsPresentationPupilModel>, MyPupilsModelToMyPupilsPresentationPupilModel>()
            .AddScoped<IMyPupilsPresentationService, MyPupilsPresentationService>()
            .AddScoped<IMyPupilsLogSink, MyPupilsLogSink>();

        
        services
            .AddSingleton<OrderMyPupilsModelPresentationHandler>()
            .AddSingleton<PaginateMyPupilsModelPresentationHandler>()
            .AddSingleton<ApplySelectionToPupilPresentationHandler>();

        // TODO implement the generic ChainedEvaluationHandlerBuilder and IEvaluator<Tin, TOut>
        services.AddSingleton<IMyPupilsPresentationModelHandler>(sp =>
        {
            var order = new ChainedEvaluationMyPupilDtosPresentationHandler(sp.GetRequiredService<OrderMyPupilsModelPresentationHandler>());

            var paginate = new ChainedEvaluationMyPupilDtosPresentationHandler(sp.GetRequiredService<PaginateMyPupilsModelPresentationHandler>());

            var applySelection = new ChainedEvaluationMyPupilDtosPresentationHandler(head: sp.GetRequiredService<ApplySelectionToPupilPresentationHandler>());

            order.ChainNext(paginate.ChainNext(applySelection));
            return order;
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
            .AddScoped<IGetMyPupilsStateQueryHandler, GetMyPupilsStateQueryHandler>()
            .AddScoped<IUpdateMyPupilsStateCommandHandler, UpdateMyPupilsStateCommandHandler>()
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
