using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils;
using DfE.GIAP.Web.Controllers.MyPupilList;
using DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation;
using DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PresenterHandler;
using DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PresenterHandler.Options;
using DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PresenterHandler.Order;
using DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PresenterHandler.Paginate;
using DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PupilSelectionState.Dto;
using DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PupilSelectionState.Mapper;
using DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PupilSelectionState.Provider;
using DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PupilSelectionState.Response;

namespace DfE.GIAP.Web.Extensions.Startup;

public static class MyPupilsPresentationExtensions
{
    public static IServiceCollection AddMyPupils(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services
            .AddMyPupilsDependencies()
            .AddScoped<IMyPupilsPresentationService, MyPupilsPresentationService>()
            .AddScoped<IPupilSelectionStateProvider, PupilSelectionStateProvider>()
            .AddScoped<IPresentPupilOptionsProvider, PresentPupilOptionsProvider>()
            .AddSingleton<IPupilDtoPresentationHandler, OrderPupilDtosPresentationHandler>()
            .AddSingleton<IPupilDtoPresentationHandler, PaginatePupilDtosPresentationHandler>()
            .AddSingleton<IMapper<PupilDtoWithPupilSelectionStateDto, PupilPresentatationViewModel>, MapPupilDtoWithSelectionStateDecoratorToPupilPresentationViewModelMapper>()
            .AddSingleton<IMapper<MyPupilsFormStateRequestDto, PresentPupilsOptions>, MapMyPupilsFormStateRequestDtoToPresentationOptions>();
            
        return services;
    }
}
