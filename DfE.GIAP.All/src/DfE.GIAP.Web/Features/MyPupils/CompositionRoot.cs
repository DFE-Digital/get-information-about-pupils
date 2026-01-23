using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.v2.Composition;
using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.v2.Evaluator;
using DfE.GIAP.Core.Common.CrossCutting.ChainOfResponsibility.v2.Handlers;
using DfE.GIAP.Core.MyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.Web.Features.MyPupils.Controllers.GetMyPupils;
using DfE.GIAP.Web.Features.MyPupils.Messaging;
using DfE.GIAP.Web.Features.MyPupils.Messaging.DataTransferObjects;
using DfE.GIAP.Web.Features.MyPupils.Messaging.Mapper;
using DfE.GIAP.Web.Features.MyPupils.PresentationService.DeletePupils;
using DfE.GIAP.Web.Features.MyPupils.PresentationService.GetPupils;
using DfE.GIAP.Web.Features.MyPupils.PresentationService.GetPupils.Mapper;
using DfE.GIAP.Web.Features.MyPupils.PresentationService.PresentationHandlers;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection.Operations;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection.Operations.ClearPupilSelections;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection.Operations.GetPupilSelections;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection.Operations.Mapper;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection.Operations.Mapper.DataTransferObjects;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection.Operations.UpdatePupilSelections;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection.Operations.UpdatePupilSelections.Handlers;
using DfE.GIAP.Web.Features.MyPupils.Services.DeletePupils;
using DfE.GIAP.Web.Features.MyPupils.Services.GetSelectedPupilIdentifiers;
using DfE.GIAP.Web.Shared.Serializer;
using DfE.GIAP.Web.Shared.Session.Abstraction;
using DfE.GIAP.Web.Shared.Session.Abstraction.Command;
using DfE.GIAP.Web.Shared.Session.Abstraction.Query;
using DfE.GIAP.Web.Shared.Session.Infrastructure.Command;
using DfE.GIAP.Web.Shared.Session.Infrastructure.Query;
using DfE.GIAP.Web.Shared.Session.Infrastructure.Serialization;

namespace DfE.GIAP.Web.Features.MyPupils;

public static class CompositionRoot
{
    public static IServiceCollection AddMyPupils(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services
            .AddMyPupilsCore();
           
        services
            .AddMyPupilsSelectionStateHandlers()
            .AddMyPupilsPresentationServices();

        return services;
    }


    private static IServiceCollection AddMyPupilsPresentationServices(this IServiceCollection services)
    {
        services.AddOptions<MyPupilsMessagingOptions>();

        // PresentationService
        services
            .AddSingleton<IMapper<MyPupilsModels, MyPupilsPresentationPupilModels>, MyPupilModelsToMyPupilsPresentationPupilModelMapper>()
            .AddSingleton<IMapper<MyPupilsModel, MyPupilsPresentationPupilModel>, MyPupilModelToMyPupilsPresentationPupilModelMapper>()
            .AddScoped<IMapper<MyPupilsPresentationResponse, MyPupilsViewModel>, MyPupilsPresentationResponseToMyPupilsViewModelMapper>()
            .AddScoped<IGetMyPupilsPresentationService, GetMyPupilsPresentationService>()
            .AddScoped<IGetSelectedPupilsUniquePupilNumbersPresentationService, GetSelectedPupilsUniquePupilNumbersPresentationService>()
            .AddScoped<IDeleteMyPupilsPresentationService, DeleteMyPupilsPresentationService>();
            

        // MessagingSink
        services
            .AddScoped<IMyPupilsMessageSink, MyPupilsTempDataMessageSink>()
            .AddSingleton<IMapper<MyPupilsMessage, MyPupilsMessageDto>, MyPupilsMessageToMyPupilsMessageDtoMapper>()
            .AddSingleton<IMapper<MyPupilsMessageDto, MyPupilsMessage>, MyPupilsMessageDtoToMyPupilsMessageMapper>();

        services
            .AddSingleton<OrderMyPupilsModelPresentationHandler>()
            .AddSingleton<PaginateMyPupilsModelPresentationHandler>()

            .AddSingleton<
                IEvaluatorV2<MyPupilsPresentationHandlerRequest, MyPupilsPresentationPupilModels>,
                EvaluatorV2<MyPupilsPresentationHandlerRequest, MyPupilsPresentationPupilModels>>()
            .AddSingleton((serviceProvider) =>
            {
                HandlerChainBuilder<
                    MyPupilsPresentationHandlerRequest,
                    MyPupilsPresentationPupilModels,
                        IEvaluationHandlerV2<MyPupilsPresentationHandlerRequest, MyPupilsPresentationPupilModels>> handlerChainBuilder =
                            HandlerChainBuilder<
                                MyPupilsPresentationHandlerRequest,
                                MyPupilsPresentationPupilModels,
                                IEvaluationHandlerV2 <MyPupilsPresentationHandlerRequest, MyPupilsPresentationPupilModels>>.Create();

                handlerChainBuilder
                    .ChainNext(serviceProvider.GetRequiredService<OrderMyPupilsModelPresentationHandler>())
                    .ChainNext(serviceProvider.GetRequiredService<PaginateMyPupilsModelPresentationHandler>());

                return handlerChainBuilder.Build();
            });

        return services;
    }

    private static IServiceCollection AddMyPupilsSelectionStateHandlers(this IServiceCollection services)
    {
        // Serailizers for State
        services
            .AddSingleton<MyPupilsPupilSelectionStateFromDtoMapper>()
            .AddSingleton<MyPupilsPupilSelectionStateToDtoMapper>()
            .AddSingleton<ISessionObjectSerializer<MyPupilsPupilSelectionState>>(sp =>
            {
                return new MappedToDataTransferObjectSessionObjectSerializer<
                    MyPupilsPupilSelectionState,
                    MyPupilsPupilSelectionStateDto>(
                        sp.GetRequiredService<MyPupilsPupilSelectionStateToDtoMapper>(),
                        sp.GetRequiredService<MyPupilsPupilSelectionStateFromDtoMapper>(),
                        sp.GetRequiredService<IJsonSerializer>());
            })
            // Query
            .AddScoped<ISessionQueryHandler<MyPupilsPupilSelectionState>, AspNetCoreSessionQueryHandler<MyPupilsPupilSelectionState>>()
            // Command
            .AddScoped<ISessionCommandHandler<MyPupilsPupilSelectionState>, AspNetCoreSessionCommandHandler<MyPupilsPupilSelectionState>>();

        services
            .AddScoped<IGetMyPupilsPupilSelectionProvider, GetMyPupilsPupilSelectionProvider>()
            .AddScoped<IClearMyPupilsPupilSelectionsHandler, ClearMyPupilsPupilSelectionsHandler>()
            .AddScoped<IUpdateMyPupilsPupilSelectionsCommandHandler, UpdateMyPupilsPupilSelectionsCommandHandler>()

            .AddScoped<IEvaluatorV2<UpdateMyPupilsSelectionStateRequest>, EvaluatorV2<UpdateMyPupilsSelectionStateRequest>>()
            .AddSingleton<SelectAllPupilsCommandHandler>()
            .AddSingleton<DeselectAllPupilsCommandHandler>()
            .AddSingleton<ManualSelectPupilsCommandHandler>()

            .AddSingleton((serviceProvider) =>
            {
                HandlerChainBuilder<
                    UpdateMyPupilsSelectionStateRequest,
                    IEvaluationHandlerV2<UpdateMyPupilsSelectionStateRequest>> handlerChainBuilder =
                        HandlerChainBuilder<
                            UpdateMyPupilsSelectionStateRequest,
                            IEvaluationHandlerV2<UpdateMyPupilsSelectionStateRequest>>
                                .Create();

                handlerChainBuilder
                    .ChainNext(serviceProvider.GetRequiredService<SelectAllPupilsCommandHandler>())
                    .ChainNext(serviceProvider.GetRequiredService<DeselectAllPupilsCommandHandler>())
                    .ChainNext(serviceProvider.GetRequiredService<ManualSelectPupilsCommandHandler>());

                return handlerChainBuilder.Build();
            });
            
        return services;
    }
}
