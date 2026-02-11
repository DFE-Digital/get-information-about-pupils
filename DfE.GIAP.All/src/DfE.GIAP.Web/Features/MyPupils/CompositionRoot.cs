using DfE.GIAP.Core.MyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.Core.Search.Application.Options.Search;
using DfE.GIAP.Web.Features.MyPupils.Controllers.GetMyPupils;
using DfE.GIAP.Web.Features.MyPupils.Messaging;
using DfE.GIAP.Web.Features.MyPupils.Messaging.DataTransferObjects;
using DfE.GIAP.Web.Features.MyPupils.Messaging.Mapper;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection.ClearPupilSelections;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection.GetPupilSelections;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection.Mapper;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection.Mapper.DataTransferObjects;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection.Options;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection.UpdatePupilSelections;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection.UpdatePupilSelections.Handlers;
using DfE.GIAP.Web.Features.MyPupils.Services.DeletePupils;
using DfE.GIAP.Web.Features.MyPupils.Services.GetPupils;
using DfE.GIAP.Web.Features.MyPupils.Services.GetPupils.Mapper;
using DfE.GIAP.Web.Features.MyPupils.Services.GetSelectedPupilUpns;
using DfE.GIAP.Web.Features.MyPupils.Services.UpsertSelectedPupils;
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
    public static IServiceCollection AddMyPupils(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);

        services
            .AddSearchOptions(configuration)
            .AddMyPupilsCore();
           
        services
            .AddMyPupilsSelectionStateHandlers()
            .AddMyPupilsPresentationServices();

        return services;
    }


    private static IServiceCollection AddMyPupilsPresentationServices(this IServiceCollection services)
    {
        services.AddOptions<MyPupilsMessagingOptions>();
        services.AddOptions<MyPupilSelectionOptions>();

        // PresentationServices
        services
            .AddSingleton<IMapper<MyPupilsModels, MyPupilsPresentationPupilModels>, MyPupilModelsToMyPupilsPresentationPupilModelMapper>()
            .AddSingleton<IMapper<MyPupilsModel, MyPupilsPresentationPupilModel>, MyPupilModelToMyPupilsPresentationPupilModelMapper>()
            .AddScoped<IMapper<MyPupilsPresentationResponse, MyPupilsViewModel>, MyPupilsPresentationResponseToMyPupilsViewModelMapper>()
            .AddScoped<IGetMyPupilsPresentationService, GetMyPupilsPresentationService>()
            .AddScoped<IGetSelectedPupilsUniquePupilNumbersPresentationService, GetSelectedPupilsUniquePupilNumbersPresentationService>()
            .AddScoped<IUpsertSelectedPupilsIdentifiersPresentationService, UpsertSelectedPupilsPresentationService>()
            .AddScoped<IDeleteMyPupilsPresentationService, DeleteMyPupilsPresentationService>();
            
        // MessagingSink
        services
            .AddScoped<IMyPupilsMessageSink, MyPupilsTempDataMessageSink>()
            .AddSingleton<IMapper<MyPupilsMessage, MyPupilsMessageDto>, MyPupilsMessageToMyPupilsMessageDtoMapper>()
            .AddSingleton<IMapper<MyPupilsMessageDto, MyPupilsMessage>, MyPupilsMessageDtoToMyPupilsMessageMapper>();

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

            .AddScoped<IEvaluator<UpdateMyPupilsSelectionStateRequest>, Evaluator<UpdateMyPupilsSelectionStateRequest>>()
            .AddSingleton<SelectAllPupilsCommandHandler>()
            .AddSingleton<DeselectAllPupilsCommandHandler>()
            .AddSingleton<ManualSelectPupilsCommandHandler>()

            .AddSingleton((serviceProvider) =>
            {
                HandlerChainBuilder<
                    UpdateMyPupilsSelectionStateRequest,
                    IEvaluationHandler<UpdateMyPupilsSelectionStateRequest>> handlerChainBuilder =
                        HandlerChainBuilder<
                            UpdateMyPupilsSelectionStateRequest,
                            IEvaluationHandler<UpdateMyPupilsSelectionStateRequest>>
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
