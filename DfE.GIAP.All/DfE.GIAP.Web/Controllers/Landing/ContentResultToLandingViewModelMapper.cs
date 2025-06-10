using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Content.Application.UseCases.GetMultipleContentByKeys;
using DfE.GIAP.Web.Controllers.Extensions;
using DfE.GIAP.Web.ViewModels;

namespace DfE.GIAP.Web.Controllers.Landing;

internal sealed class ContentResultToLandingViewModelMapper : IMapper<GetContentByPageKeyUseCaseResponse, LandingViewModel>
{
    public LandingViewModel Map(GetContentByPageKeyUseCaseResponse input)
    {
        return new()
        {
            LandingResponse = input.ContentResultItems.FilterByContentKeyOrEmpty("LandingSummary"),
            PlannedMaintenanceResponse = input.ContentResultItems.FilterByContentKeyOrEmpty("PlannedMaintenance"),
            FAQResponse = input.ContentResultItems.FilterByContentKeyOrEmpty("FrequentlyAskedQuestions"),
            PublicationScheduleResponse = input.ContentResultItems.FilterByContentKeyOrEmpty("PublicationSchedule")
        };
    }
}
