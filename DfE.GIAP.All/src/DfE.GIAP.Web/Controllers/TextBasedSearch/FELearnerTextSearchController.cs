using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Common.Helpers;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Downloads.Application.Enums;
using DfE.GIAP.Core.Downloads.Application.UseCases.GetAvailableDatasetsForPupils;
using DfE.GIAP.Core.Models.Search;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Domain.Models.User;
using DfE.GIAP.Service.Download;
using DfE.GIAP.Service.MPL;
using DfE.GIAP.Service.Search;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Helpers;
using DfE.GIAP.Web.Helpers.SearchDownload;
using DfE.GIAP.Web.Helpers.SelectionManager;
using DfE.GIAP.Web.Providers.Session;
using DfE.GIAP.Web.ViewModels.Search;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Web.Controllers.TextBasedSearch;

[Route(Routes.Application.Search)]
public class FELearnerTextSearchController : BaseLearnerTextSearchController
{
    public override string PageHeading => ApplicationLabels.SearchFEWithoutUlnPageHeading;
    public override string SearchSessionKey => Global.FENonUlnSearchSessionKey;
    public override string SearchFiltersSessionKey => Global.FENonUlnSearchFiltersSessionKey;
    public override string SortDirectionKey => Global.FENonUlnSortDirectionSessionKey;
    public override string SortFieldKey => Global.FENonUlnSortFieldSessionKey;
    public override string DownloadLinksPartial => Global.FENonUlnDownloadLinksView;
    public override string SearchLearnerNumberAction => Routes.FurtherEducation.LearnerNumberSearch;
    public override string RedirectUrlFormAction => Global.FELearnerTextSearchAction;
    public override string LearnerTextDatabaseAction => Global.FELearnerTextSearchAction;
    public override string LearnerTextDatabaseName => Global.FELearnerTextSearchAction;
    public override string RedirectFrom => Routes.FurtherEducation.LearnerTextSearch;

    public override string SurnameFilterUrl => Routes.FurtherEducation.NonULNSurnameFilter;
    public override string DobFilterUrl => Routes.FurtherEducation.NonULNDobFilter;
    public override string ForenameFilterUrl => Routes.FurtherEducation.NonULNForenameFilter;
    public override string MiddlenameFilterUrl => "";
    public override string GenderFilterUrl => Routes.FurtherEducation.NonULNGenderFilter;
    public override string SexFilterUrl => Routes.FurtherEducation.NonULNSexFilter;

    public override string FormAction => Routes.FurtherEducation.LearnerTextSearch;
    public override string RemoveActionUrl => $"/{Routes.Application.Search}/{Routes.FurtherEducation.LearnerTextSearch}";
    public override AzureSearchIndexType IndexType => AzureSearchIndexType.FurtherEducation;
    public override string SearchView => Global.NonUpnSearchView;

    public override string SearchLearnerNumberController => Routes.Application.Search;
    public override int MyPupilListLimit => _appSettings.NonUpnNPDMyPupilListLimit; //Not valid for FE so arbitrarily set to default non UPN limit
    public override string SearchAction => Global.FELearnerTextSearchAction;
    public override string SearchController => Global.FELearnerTextSearchController;
    public override ReturnRoute ReturnRoute => ReturnRoute.NonUniqueLearnerNumber;
    public override string LearnerTextSearchController => Global.FELearnerTextSearchController;
    public override string LearnerTextSearchAction => Global.FELearnerTextSearchAction;
    public override string LearnerNumberAction => Routes.NationalPupilDatabase.NationalPupilDatabaseLearnerNumber;

    public override bool ShowGender => _appSettings.FeUseGender;
    public override bool ShowLocalAuthority => false;
    public override string InvalidUPNsConfirmationAction => "";
    public override string LearnerNumberLabel => Global.FELearnerNumberLabel;
    public override bool ShowMiddleNames => false;

    public override string DownloadSelectedLink => ApplicationLabels.DownloadSelectedFurtherEducationLink;

    private readonly ILogger<FELearnerTextSearchController> _logger;
    private readonly IDownloadService _downloadService;

    private readonly IUseCase<GetAvailableDatasetsForPupilsRequest, GetAvailableDatasetsForPupilsResponse> _getAvailableDatasetsForPupilsUseCase;


    public FELearnerTextSearchController(
       ILogger<FELearnerTextSearchController> logger,
       IOptions<AzureAppSettings> azureAppSettings,
       IPaginatedSearchService paginatedSearch,
       IMyPupilListService mplService,
       ITextSearchSelectionManager selectionManager,
       ISessionProvider sessionProvider,
       IDownloadService downloadService,
       IUseCase<GetAvailableDatasetsForPupilsRequest, GetAvailableDatasetsForPupilsResponse> getAvailableDatasetsForPupilsUseCase)
       : base(logger,
             paginatedSearch,
             mplService,
             selectionManager,
             azureAppSettings,
             sessionProvider)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;

        ArgumentNullException.ThrowIfNull(downloadService);
        _downloadService = downloadService;

        ArgumentNullException.ThrowIfNull(getAvailableDatasetsForPupilsUseCase);
        _getAvailableDatasetsForPupilsUseCase = getAvailableDatasetsForPupilsUseCase;
    }


    [Route(Routes.FurtherEducation.LearnerTextSearch)]
    [HttpGet]
    public async Task<IActionResult> FurtherEducationNonUlnSearch(bool? returnToSearch)
    {
        _logger.LogInformation("Further education non ULN search GET method called");
        return await Search(returnToSearch);
    }

    [Route(Routes.FurtherEducation.LearnerTextSearch)]
    [HttpPost]
    public async Task<IActionResult> FurtherEducationNonUlnSearch(
        LearnerTextSearchViewModel model,
        string surnameFilter,
        string middlenameFilter,
        string forenameFilter,
        string searchByRemove,
        [FromQuery] string sortField,
        [FromQuery] string sortDirection,
        bool calledByController = false)
    {
        _logger.LogInformation("Further education non ULN search POST method called");
        model.ShowHiddenUPNWarningMessage = false;
        return await Search(model, surnameFilter, middlenameFilter, forenameFilter,
                                 searchByRemove, model.PageNumber,
                                 ControllerContext.HttpContext.Request.Query.ContainsKey("pageNumber"),
                                 calledByController, sortField, sortDirection,
                                 ControllerContext.HttpContext.Request.Query.ContainsKey("reset"));
    }


    [Route(Routes.FurtherEducation.NonULNDobFilter)]
    [HttpPost]
    public async Task<IActionResult> DobFilter(LearnerTextSearchViewModel model)
    {
        return await DobSearchFilter(model);
    }

    [Route(Routes.FurtherEducation.NonULNSurnameFilter)]
    [HttpPost]
    public async Task<IActionResult> SurnameFilter(LearnerTextSearchViewModel model, string surnameFilter)
    {
        return await SurnameSearchFilter(model, surnameFilter);
    }

    [Route(Routes.FurtherEducation.NonULNForenameFilter)]
    [HttpPost]
    public async Task<IActionResult> ForenameFilter(LearnerTextSearchViewModel model, string forenameFilter)
    {
        return await ForenameSearchFilter(model, forenameFilter);
    }

    [Route(Routes.FurtherEducation.NonULNGenderFilter)]
    [HttpPost]
    public async Task<IActionResult> GenderFilter(LearnerTextSearchViewModel model)
    {
        return await GenderSearchFilter(model);
    }

    [Route(Routes.FurtherEducation.NonULNSexFilter)]
    [HttpPost]
    public async Task<IActionResult> SexFilter(LearnerTextSearchViewModel model)
    {
        return await SexSearchFilter(model);
    }

    [Route(Routes.DownloadSelectedNationalPupilDatabaseData)]
    [HttpPost]
    public async Task<IActionResult> DownloadSelectedFurtherEducationData(
        string selectedPupil,
        string searchText)
    {
        LearnerDownloadViewModel searchDownloadViewModel = new()
        {
            SelectedPupils = selectedPupil,
            LearnerNumber = selectedPupil,
            ErrorDetails = (string)TempData["ErrorDetails"],
            SelectedPupilsCount = 1,
            DownloadFileType = DownloadFileType.CSV,
            ShowTABDownloadType = false
        };


        GetAvailableDatasetsForPupilsRequest request = new(
            DownloadType: Core.Downloads.Application.Enums.DownloadType.FurtherEducation,
            SelectedPupils: new List<string> { selectedPupil },
            AuthorisationContext: new HttpClaimsAuthorisationContext(User));
        GetAvailableDatasetsForPupilsResponse response = await _getAvailableDatasetsForPupilsUseCase.HandleRequestAsync(request);

        foreach (AvailableDatasetResult datasetResult in response.AvailableDatasets)
        {
            searchDownloadViewModel.SearchDownloadDatatypes.Add(new SearchDownloadDataType
            {
                Name = StringHelper.StringValueOfEnum(datasetResult.Dataset),
                Value = datasetResult.Dataset.ToString(),
                Disabled = !datasetResult.HasData,
                CanDownload = datasetResult.CanDownload
            });
        }

        ModelState.Clear();

        searchDownloadViewModel.TextSearchViewModel.PageLearnerNumbers = selectedPupil;
        searchDownloadViewModel.SearchAction = Global.FELearnerTextSearchAction;
        searchDownloadViewModel.DownloadRoute = Routes.FurtherEducation.DownloadNonUlnFile;
        searchDownloadViewModel.RedirectRoute = Routes.FurtherEducation.LearnerTextSearch;
        searchDownloadViewModel.TextSearchViewModel = new LearnerTextSearchViewModel() { LearnerNumberLabel = LearnerNumberLabel, SearchText = searchText };
        PopulateNavigation(searchDownloadViewModel.TextSearchViewModel);

        searchDownloadViewModel.SearchResultPageHeading = PageHeading;
        return View(Global.NonLearnerNumberDownloadOptionsView, searchDownloadViewModel);
    }

    [Route(Routes.FurtherEducation.DownloadNonUlnFile)]
    [HttpPost]
    public async Task<IActionResult> DownloadFurtherEducationFile(LearnerDownloadViewModel model)
    {
        if (!string.IsNullOrEmpty(model.LearnerNumber))
        {
            var selectedPupils = model.LearnerNumber.Split(',');

            if (model.SelectedDownloadOptions == null)
            {
                model.ErrorDetails = Messages.Search.Errors.SelectOneOrMoreDataTypes;
            }
            else if (model.DownloadFileType != DownloadFileType.None)
            {
                var downloadFile = await _downloadService.GetFECSVFile(selectedPupils, model.SelectedDownloadOptions, true, AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId()), ReturnRoute.NonUniqueLearnerNumber).ConfigureAwait(false);

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
                    model.ErrorDetails = Messages.Downloads.Errors.NoDataForSelectedPupils;
                }
            }
            else
            {
                model.ErrorDetails = Messages.Search.Errors.SelectFileType;
            }

            TempData["ErrorDetails"] = model.ErrorDetails;

            if (this.HttpContext.Session.Keys.Contains(SearchSessionKey))
                model.TextSearchViewModel.SearchText = this.HttpContext.Session.GetString(SearchSessionKey);

            return await DownloadSelectedFurtherEducationData(model.SelectedPupils, model.TextSearchViewModel?.SearchText);
        }

        return RedirectToAction(Global.FELearnerTextSearchAction, Global.FELearnerTextSearchController);
    }

    [Route(Routes.FurtherEducation.DownloadNonUlnRequest)]
    [HttpPost]
    public async Task<IActionResult> ToDownloadSelectedFEDataULN(LearnerTextSearchViewModel model)
    {
        SetSelections(
            model.PageLearnerNumbers.Split(','),
            model.SelectedPupil);

        var selectedPupil = GetSelected();

        if (string.IsNullOrEmpty(selectedPupil))
        {
            model.NoPupil = true;
            model.NoPupilSelected = true;
            model.ErrorDetails = Messages.Downloads.Errors.NoLearnerSelected;
            return await FurtherEducationNonUlnSearch(model, null, null, null, null, null, null);
        }

        return await DownloadSelectedFurtherEducationData(selectedPupil, model.SearchText);
    }
}
