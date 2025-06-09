using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Constants.Messages.Downloads;
using DfE.GIAP.Common.Constants.Messages.Search;
using DfE.GIAP.Common.Constants.Search.FurtherEducation;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Common.Helpers;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Service.Content;
using DfE.GIAP.Service.Download;
using DfE.GIAP.Service.MPL;
using DfE.GIAP.Service.Search;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Helpers.Search;
using DfE.GIAP.Web.Helpers.SearchDownload;
using DfE.GIAP.Web.Helpers.SelectionManager;
using DfE.GIAP.Web.ViewModels.Search;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DfE.GIAP.Web.Controllers.LearnerNumber;

[FeatureGate(FeatureFlags.FurtherEducation)]
[Route(Routes.Application.Search)]
public class FELearnerNumberController : BaseLearnerNumberController
{
    private readonly ILogger<FELearnerNumberController> _logger;
    private readonly IDownloadService _downloadService;
    private readonly AzureAppSettings _appSettings;

    public override string PageHeading => UniqueLearnerNumberLabels.SearchPupilUpnPageHeading;
    public override string SearchAction => "PupilUlnSearch";
    public override string FullTextLearnerSearchController => Global.FELearnerTextSearchController;
    public override string FullTextLearnerSearchAction => Global.FELearnerTextSearchAction;
    public override string DownloadLinksPartial => "~/Views/Shared/LearnerNumber/_SearchFurtherEducationDownloadLinks.cshtml";

    public override AzureSearchIndexType IndexType => AzureSearchIndexType.FurtherEducation;
    public override string SearchSessionKey => "SearchULN_SearchText";
    public override string SearchSessionSortField => "SearchULN_SearchTextSortField";
    public override string SearchSessionSortDirection => "SearchULN_SearchTextSortDirection";
    public override bool ShowLocalAuthority => false;
    public override bool ShowMiddleNames => false;
    public override string DownloadSelectedLink => ApplicationLabel.DownloadSelectedFurtherEducationLink;
    public override string LearnerNumberLabel => Global.FELearnerNumberLabel;

    //below 2 are n/a for FE
    public override int MyPupilListLimit => int.MaxValue;

    public override string InvalidUPNsConfirmationAction => "";


    public FELearnerNumberController(ILogger<FELearnerNumberController> logger,
        IDownloadService downloadService,
        IPaginatedSearchService paginatedSearch,
        IMyPupilListService mplService,
        ISelectionManager selectionManager,
        IContentService contentService,
        IOptions<AzureAppSettings> azureAppSettings)
        : base(logger, paginatedSearch, mplService, selectionManager, contentService, azureAppSettings)
    {
        _logger = logger ??
            throw new ArgumentNullException(nameof(logger));
        _downloadService = downloadService ??
            throw new ArgumentNullException(nameof(downloadService));
        _appSettings = azureAppSettings.Value;
    }


    [Route(Routes.FurtherEducation.LearnerNumberSearch)]
    [HttpGet]
    public async Task<IActionResult> PupilUlnSearch(bool? returnToSearch)
    {
        if (!(User.IsEstablishmentWithFurtherEducation() || User.IsEstablishmentWithAccessToULNPages() || User.IsDfeUser()))
        {
            return RedirectToAction("Error", "Home");
        }

        _logger.LogInformation("Pupil Uln Search GET method called");
        return await Search(returnToSearch);
    }

    [Route(Routes.FurtherEducation.LearnerNumberSearch)]
    [HttpPost]
    public async Task<IActionResult> PupilUlnSearch(
        [FromForm] LearnerNumberSearchViewModel model,
        [FromQuery] int pageNumber,
        [FromQuery] string sortField,
        [FromQuery] string sortDirection,
        bool calledByController = false)
    {
        if (!(User.IsEstablishmentWithFurtherEducation() || User.IsEstablishmentWithAccessToULNPages() || User.IsDfeUser()))
        {
            return RedirectToAction("Error", "Home");
        }

        _logger.LogInformation("Pupil Unique Learner Number  Search POST method called");

        return await Search(
        model,
        pageNumber,
        sortField,
        sortDirection,
        !ControllerContext.HttpContext.Request.Query.ContainsKey("pageNumber"),
        calledByController,
        ControllerContext.HttpContext.Request.Query.ContainsKey("reset"));
    }


    [Route(UniqueLearnerNumberLabels.ToDownloadSelectedULNData)]
    [HttpPost]
    public async Task<IActionResult> ToDownloadSelectedULNData(LearnerNumberSearchViewModel searchViewModel)
    {
        SetSelections(
         searchViewModel.PageLearnerNumbers.Split(','),
         searchViewModel.SelectedPupil);

        var selectedPupils = GetSelected(searchViewModel.LearnerNumber.FormatLearnerNumbers());

        if (selectedPupils.Count == 0)
        {
            searchViewModel.NoPupil = true;
            searchViewModel.NoPupilSelected = true;
            return await PupilUlnSearch(searchViewModel, searchViewModel.PageNumber, this.HttpContext.Session.GetString(SearchSessionSortField), this.HttpContext.Session.GetString(SearchSessionSortDirection), true);
        }

        var joinedSelectedPupils = String.Join(',', selectedPupils);
        return await DownloadSelectedUlnDatabaseData(joinedSelectedPupils, searchViewModel.LearnerNumber, selectedPupils.Count);
    }

    [Route(UniqueLearnerNumberLabels.DownloadSelectedUlnData)]
    [HttpPost]
    public async Task<IActionResult> DownloadSelectedUlnDatabaseData(LearnerDownloadViewModel model)
    {
        if (!String.IsNullOrEmpty(model.SelectedPupils))
        {
            var selectedPupils = model.SelectedPupils.Split(',');
            if (model.SelectedDownloadOptions == null)
            {
                model.ErrorDetails = SearchErrorMessages.SelectOneOrMoreDataTypes;
            }
            else if (model.DownloadFileType != DownloadFileType.None)
            {
                var downloadFile = await _downloadService.GetFECSVFile(selectedPupils, model.SelectedDownloadOptions, true, AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId()), ReturnRoute.UniqueLearnerNumber).ConfigureAwait(false);

                if (downloadFile == null)
                {
                    return RedirectToAction(Routes.Application.Error, Routes.Application.Home);
                }

                if (downloadFile.Bytes != null)
                {
                    model.ErrorDetails = null;
                    return SearchDownloadHelper.DownloadFile(downloadFile);
                }
                else
                {
                    model.ErrorDetails = DownloadErrorMessages.NoDataForSelectedPupils;
                }
            }
            else
            {
                model.ErrorDetails = SearchErrorMessages.SelectFileType;
            }

            TempData["ErrorDetails"] = model.ErrorDetails;

            return await DownloadSelectedUlnDatabaseData(model.SelectedPupils, model.LearnerNumber, model.SelectedPupilsCount);
        }
        return RedirectToAction(SearchAction, UniqueLearnerNumberLabels.SearchUlnControllerName);
    }

    [Route(UniqueLearnerNumberLabels.DownloadSelectedUlnData)]
    [HttpGet]
    public async Task<IActionResult> DownloadSelectedUlnDatabaseData(string selectedPupilsJoined, string uln, int selectedPupilsCount)
    {
        var searchDownloadViewModel = new LearnerDownloadViewModel
        {
            SelectedPupils = selectedPupilsJoined,
            LearnerNumber = uln,
            ErrorDetails = (string)TempData["ErrorDetails"],
            SelectedPupilsCount = selectedPupilsCount,
            DownloadFileType = DownloadFileType.CSV,
            ShowTABDownloadType = false
        };

        SearchDownloadHelper.AddUlnDownloadDataTypes(searchDownloadViewModel, User, User.GetOrganisationHighAge(), User.IsDfeUser());
        LearnerNumberSearchViewModel.MaximumLearnerNumbersPerSearch = _appSettings.MaximumULNsPerSearch;
        ModelState.Clear();

        PopulateNavigation(searchDownloadViewModel.NumberSearchViewModel);
        searchDownloadViewModel.NumberSearchViewModel.LearnerNumber = selectedPupilsJoined.Replace(",", "\r\n");
        searchDownloadViewModel.SearchAction = SearchAction;
        searchDownloadViewModel.DownloadRoute = UniqueLearnerNumberLabels.DownloadSelectedUlnData;
        searchDownloadViewModel.NumberSearchViewModel.LearnerNumberLabel = "ULN";

        var selectedPupils = selectedPupilsJoined.Split(',');
        if (selectedPupils.Length < _appSettings.DownloadOptionsCheckLimit)
        {
            var downloadTypeArray = searchDownloadViewModel.SearchDownloadDatatypes.Select(d => d.Value).ToArray();
            var disabledTypes = await _downloadService.CheckForFENoDataAvailable(selectedPupils, downloadTypeArray, AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId())).ConfigureAwait(false);
            SearchDownloadHelper.DisableDownloadDataTypes(searchDownloadViewModel, disabledTypes);
        }
        searchDownloadViewModel.SearchResultPageHeading = PageHeading;
        //need to update searchbox to show uln wording
        return View(Global.DownloadNPDOptionsView, searchDownloadViewModel);
    }


    protected override async Task<IActionResult> ReturnToPage(LearnerNumberSearchViewModel model)
    {
        return await PupilUlnSearch(model, model.PageNumber, this.HttpContext.Session.GetString(SearchSessionSortField), this.HttpContext.Session.GetString(SearchSessionSortDirection), true);
    }

    protected override bool ValidateLearnerNumber(string learnerNumber)
    {
        return ValidationHelper.IsValidUln(learnerNumber);
    }

    protected override string GenerateValidationMessage()
    {
        return PupilHelper.GenerateValidationMessageUlnSearch(ModelState);
    }
}
