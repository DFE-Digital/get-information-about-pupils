using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Common.Helpers;
using DfE.GIAP.Core.MyPupils.Application.UseCases.AddPupilsToMyPupils;
using DfE.GIAP.Service.Search;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Features.Downloads.Services;
using DfE.GIAP.Web.Helpers.SelectionManager;
using DfE.GIAP.Web.Shared.Serializer;
using DfE.GIAP.Web.ViewModels.Search;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Web.Controllers.LearnerNumber;

[Route(Routes.Application.Search)]
public class PupilPremiumLearnerNumberController : BaseLearnerNumberController
{
    private readonly ILogger<PupilPremiumLearnerNumberController> _logger;
    private readonly IDownloadPupilPremiumPupilDataService _downloadPupilPremiumDataForPupilsService;

    public override string PageHeading => ApplicationLabels.SearchPupilPremiumWithUpnPageHeading;
    public override string SearchAction => "PupilPremium";
    public override string FullTextLearnerSearchController => Global.PPNonUpnController;
    public override string FullTextLearnerSearchAction => "NonUpnPupilPremiumDatabase";
    public override string InvalidUPNsConfirmationAction => "PPInvalidUPNsConfirmation";
    public override string DownloadLinksPartial => "~/Views/Shared/LearnerNumber/_SearchPupilPremiumDownloadLinks.cshtml";
    public override AzureSearchIndexType IndexType => AzureSearchIndexType.PupilPremium;
    public override string SearchSessionKey => "SearchPPUPN_SearchText";
    public override string SearchSessionSortField => "SearchPPUPN_SearchTextSortField";
    public override string SearchSessionSortDirection => "SearchPPUPN_SearchTextSortDirection";
    public override string DownloadSelectedLink => ApplicationLabels.DownloadSelectedPupilPremiumDataLink;

    public PupilPremiumLearnerNumberController(
        ILogger<PupilPremiumLearnerNumberController> logger,
        IPaginatedSearchService paginatedSearch,
        ISelectionManager selectionManager,
        IOptions<AzureAppSettings> azureAppSettings,
        IUseCaseRequestOnly<AddPupilsToMyPupilsRequest> addPupilsToMyPupilsUseCase,
        IJsonSerializer jsonSerializer,
        IDownloadPupilPremiumPupilDataService downloadPupilPremiumDataForPupilsService)
        : base(logger, paginatedSearch, selectionManager, azureAppSettings, addPupilsToMyPupilsUseCase, jsonSerializer)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;

        ArgumentNullException.ThrowIfNull(downloadPupilPremiumDataForPupilsService);
        _downloadPupilPremiumDataForPupilsService = downloadPupilPremiumDataForPupilsService;
    }


    [Route(Routes.Application.PupilPremium)]
    [HttpGet]
    public async Task<IActionResult> PupilPremium(bool? returnToSearch)
    {
        _logger.LogInformation("Pupil Premium Upn GET method called");
        return await Search(returnToSearch);
    }

    [Route(Routes.Application.PupilPremium)]
    [HttpPost]
    public async Task<IActionResult> PupilPremium(
        [FromForm] LearnerNumberSearchViewModel model,
        [FromQuery] int pageNumber,
        [FromQuery] string sortField,
        [FromQuery] string sortDirection,
        bool calledByController = false)
    {
        _logger.LogInformation("Pupil Premium Upn POST method called");
        return await Search(
            model,
            pageNumber,
            sortField,
            sortDirection,
            !ControllerContext.HttpContext.Request.Query.ContainsKey("pageNumber"),
            calledByController,
            ControllerContext.HttpContext.Request.Query.ContainsKey("reset"));
    }


    [HttpPost]
    [Route(Routes.PPInvalidUPNs)]
    public async Task<IActionResult> PPInvalidUPNs(InvalidLearnerNumberSearchViewModel model)
    {
        return await InvalidUPNs(model);
    }

    [HttpPost]
    [Route(Routes.PPInvalidUPNsConfirmation)]
    public async Task<IActionResult> PPInvalidUPNsConfirmation(InvalidLearnerNumberSearchViewModel model)
    {
        return await InvalidUPNsConfirmation(model);
    }


    [HttpPost]
    [Route("add-pp-to-my-pupil-list")]
    public async Task<IActionResult> PPAddToMyPupilList(LearnerNumberSearchViewModel model)
    {
        return await AddToMyPupilList(model);
    }


    [Route(Routes.PupilPremium.LearnerNumberDownloadRequest)]
    [HttpPost]
    public async Task<IActionResult> ToDownloadSelectedPupilPremiumDataUPN(LearnerNumberSearchViewModel searchViewModel, CancellationToken ctx = default)
    {
        SetSelections(
            searchViewModel.PageLearnerNumbers.Split(','),
            searchViewModel.SelectedPupil);

        HashSet<string> selectedPupils = GetSelected(searchViewModel.LearnerNumberIds.FormatLearnerNumbers());

        if (selectedPupils.Count == 0)
        {
            searchViewModel.NoPupil = true;
            searchViewModel.NoPupilSelected = true;

            return await PupilPremium(
                searchViewModel,
                searchViewModel.PageNumber,
                HttpContext.Session.GetString(SearchSessionSortField),
                HttpContext.Session.GetString(SearchSessionSortDirection),
                true);
        }

        DownloadPupilPremiumFilesResponse result =
            await _downloadPupilPremiumDataForPupilsService.DownloadAsync(
                pupilUpns: selectedPupils,
                downloadEventType: Core.Common.CrossCutting.Logging.Events.DownloadType.Search,
                ctx);

        if (result.HasData)
        {
            return result.GetResult();
        }

        searchViewModel.ErrorDetails = Messages.Downloads.Errors.NoDataForSelectedPupils;

        return await PupilPremium(
            searchViewModel,
            searchViewModel.PageNumber,
            HttpContext.Session.GetString(SearchSessionSortField),
            HttpContext.Session.GetString(SearchSessionSortDirection),
            true);
    }


    protected override async Task<IActionResult> ReturnToPage(LearnerNumberSearchViewModel model)
    {
        return await PupilPremium(model, model.PageNumber, model.SortField, model.SortDirection, true);
    }
}
