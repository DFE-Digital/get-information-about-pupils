using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.Web.Features.MyPupils.Controllers.GetMyPupils;
using DfE.GIAP.Web.Features.MyPupils.Messaging;
using DfE.GIAP.Web.Features.MyPupils.Messaging.DataTransferObjects;
using DfE.GIAP.Web.Features.MyPupils.Messaging.Mapper;
using DfE.GIAP.Web.Features.MyPupils.PresentationService;
using DfE.GIAP.Web.Features.MyPupils.PresentationService.Mapper;
using DfE.GIAP.Web.Features.MyPupils.PresentationService.PresentationHandlers;
using DfE.GIAP.Web.Features.MyPupils.SelectionState;
using DfE.GIAP.Web.Features.MyPupils.SelectionState.ClearSelections;
using DfE.GIAP.Web.Features.MyPupils.SelectionState.GetPupilSelections;
using DfE.GIAP.Web.Features.MyPupils.SelectionState.Mapper;
using DfE.GIAP.Web.Features.MyPupils.SelectionState.Mapper.DataTransferObjects;
using DfE.GIAP.Web.Features.MyPupils.SelectionState.UpdatePupilSelections;
using DfE.GIAP.Web.Session.Abstraction;
using DfE.GIAP.Web.Session.Abstraction.Command;
using DfE.GIAP.Web.Session.Abstraction.Query;
using DfE.GIAP.Web.Session.Infrastructure.AspNetCore;
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
            .AddMyPupilsPresentationServices();

        return services;
    }


    private static IServiceCollection AddMyPupilsPresentationServices(this IServiceCollection services)
    {
        services.AddOptions<MyPupilsMessagingOptions>();

        // PresentationService
        services
            .AddSingleton<IMapper<MyPupilsPresentationResponse, MyPupilsViewModel>, MyPupilsPresentationResponseToMyPupilsViewModelMapper>()
            .AddSingleton<IMapper<MyPupilsModels, MyPupilsPresentationPupilModels>, MyPupilModelsToMyPupilsPresentationPupilModelMapper>()
            .AddSingleton<IMapper<MyPupilsModel, MyPupilsPresentationPupilModel>, MyPupilModelToMyPupilsPresentationPupilModelMapper>()
            .AddScoped<IMyPupilsPresentationService, MyPupilsPresentationService>();

        // MessagingSink
        services
            .AddScoped<IMyPupilsMessageSink, MyPupilsMessageSink>()
            .AddSingleton<IMapper<MyPupilsMessage, MyPupilsMessageDto>, MyPupilsMessageToMyPupilsMessageDtoMapper>()
            .AddSingleton<IMapper<MyPupilsMessageDto, MyPupilsMessage>, MyPupilsMessageDtoToMyPupilsMessageMapper>();

        // TODO implement the generic ChainedEvaluationHandlerBuilder and IEvaluator<Tin, TOut>
        services
            .AddSingleton<OrderMyPupilsModelPresentationHandler>()
            .AddSingleton<PaginateMyPupilsModelPresentationHandler>()
            .AddSingleton<ApplySelectionToPupilPresentationHandler>()
            .AddSingleton<IMyPupilsPresentationModelHandler>(sp =>
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
        services// Serailizers for State
            .AddSingleton<MyPupilsPupilSelectionStateFromDtoMapper>()
            .AddSingleton<MyPupilsPupilSelectionStateToDtoMapper>()
            .AddSingleton<ISessionObjectSerializer<MyPupilsPupilSelectionState>>(sp =>
            {
                return new MappedToDataTransferObjectSessionObjectSerializer<MyPupilsPupilSelectionState, MyPupilsPupilSelectionStateDto>(
                    sp.GetRequiredService<MyPupilsPupilSelectionStateToDtoMapper>(),
                    sp.GetRequiredService<MyPupilsPupilSelectionStateFromDtoMapper>());
            })
            // Session handlers
            .AddScoped<ISessionQueryHandler<MyPupilsPupilSelectionState>, AspNetCoreSessionQueryHandler<MyPupilsPupilSelectionState>>()
            .AddScoped<ISessionCommandHandler<MyPupilsPupilSelectionState>, AspNetCoreSessionCommandHandler<MyPupilsPupilSelectionState>>();

        services
            .AddScoped<IGetMyPupilsPupilSelectionProvider, GetMyPupilsPupilSelectionProvider>()
            .AddScoped<IClearMyPupilsPupilSelectionsHandler, ClearMyPupilsPupilSelectionsHandler>()
            .AddScoped<IUpdateMyPupilsPupilSelectionsCommandHandler, UpdateMyPupilsPupilSelectionsCommandHandler>();
            
        return services;
    }
}
