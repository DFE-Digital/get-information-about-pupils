using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Constants.Messages.Downloads;
using DfE.GIAP.Common.Constants.Messages.Search;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Common.Helpers;
using DfE.GIAP.Common.Helpers.Rbac;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Domain.Models.MPL;
using DfE.GIAP.Domain.Models.User;
using DfE.GIAP.Domain.Search.Learner;
using DfE.GIAP.Service.Common;
using DfE.GIAP.Service.Download;
using DfE.GIAP.Service.Download.CTF;
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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DfE.GIAP.Web.Controllers.Search;

[Route(Routes.Application.MyPupilList)]
public class SearchMyPupilListController : Controller
{
    public const int PAGESIZE = 20;
    public const string MISSING_LEARNER_NUMBERS_KEY = "missingLearnerNumbers";

    private readonly ILogger<SearchMyPupilListController> _logger;
    private readonly ICommonService _commonService;
    private readonly IPaginatedSearchService _paginatedSearch;
    private readonly ISelectionManager _selectionManager;
    private readonly IMyPupilListService _mplService;
    private readonly IDownloadCommonTransferFileService _ctfService;
    private readonly IDownloadService _downloadService;
    private readonly AzureAppSettings _appSettings;
    public string PageHeading => ApplicationLabel.SearchMyPupilListPageHeading;
    public string SearchAction => "MyPupilList";
    public string DownloadLinksPartial => "~/Views/Search/MyPupilList/_MyPupilListDownloadLinks.cshtml";
    public AzureSearchIndexType NPDIndexType => AzureSearchIndexType.NPD;
    public AzureSearchIndexType PPIndexType => AzureSearchIndexType.PupilPremium;

    public string SortFieldSessionKey = "SearchMPL_SortField";
    public string SortDirectionSessionKey = "SearchMPL_SortDirection";


    public SearchMyPupilListController(
        ILogger<SearchMyPupilListController> logger,
        IPaginatedSearchService paginatedSearch,
        IMyPupilListService mplService,
        ISelectionManager selectionManager,
        IDownloadCommonTransferFileService ctfService,
        IDownloadService downloadService,
        ICommonService commonService,
        IOptions<AzureAppSettings> azureAppSettings)
    {
        _logger = logger;
        _commonService = commonService;
        _paginatedSearch = paginatedSearch;
        _selectionManager = selectionManager;
        _mplService = mplService;
        _ctfService = ctfService;
        _appSettings = azureAppSettings.Value;
        _downloadService = downloadService;
    }


    [HttpGet]
    public Task<IActionResult> MyPupilList(bool returnToMPL = false)
    {
        _logger.LogInformation("My pupil list GET method is called");
        return Search(returnToMPL);
    }

    [HttpPost]
    public Task<IActionResult> MyPupilList(
        SearchMyPupilListViewModel model,
        [FromQuery] int pageNumber,
        bool calledByController = false,
        bool failedDownload = false)
    {
        _logger.LogInformation("My pupil list Upn POST method called");
        return Search(
            model,
            pageNumber,
            ControllerContext.HttpContext.Request.Query.ContainsKey("pageNumber"),
            calledByController,
            failedDownload);
    }

    /// <summary>
    /// Remove selected UPNs from user's MPL
    /// </summary>
    /// <param name="model">Search Model, items to be removed are stored in model.UPN</param>
    /// <returns>MyPupilList action</returns>
    [HttpPost]
    [Route(Routes.SearchMyPupilList.RemoveSelected)]
    public async Task<IActionResult> RemoveSelected(SearchMyPupilListViewModel model)
    {
        _logger.LogInformation("Remove from my pupil list POST method is called");

        string[] pageLearnerNumbers = model.PageLearnerNumbers.Split(',');
        SetSelections(pageLearnerNumbers, model.SelectedPupil);

        var selectedPupils = GetSelected(model.Upn.FormatLearnerNumbers());

        if (selectedPupils == null || selectedPupils.Count == 0)
        {
            model.NoPupilSelected = true;
            return await MyPupilList(model, model.PageNumber);
        }

        var userID = User.GetUserId();
        var learnerList = await GetLearnerListForCurrentUser();
        var decryptedSelectedPupils = RbacHelper.DecryptUpnCollection(selectedPupils);
        var userProfile = new UserProfile
        {
            UserId = userID,
            IsPupilListUpdated = true,
            MyPupilList = learnerList.Where(x => !decryptedSelectedPupils.Contains(x.PupilId)).ToList()
        };

        _ = await _commonService.CreateOrUpdateUserProfile(userProfile, AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId()));

        model.Upn = GetMyPupilListStringSeparatedBy(myPupilList: userProfile.MyPupilList, separator: "\n");
        model.Removed = true;

        float pagesRemainingAfterRemovals =
            NumberOfPagesRemainingAfterSelectedPupilsRemoved(model.Total, selectedPupils.Count);

        SetRevisedCurrentPageNumber(pagesRemainingAfterRemovals, model);

        return (pagesRemainingAfterRemovals != 0) ?
            await MyPupilList(model, model.PageNumber, true) : await MyPupilList();
    }


    [Route(Routes.DownloadCommonTransferFile.DownloadCommonTransferFileAction)]
    [HttpPost]
    public async Task<IActionResult> ToDownloadCommonTransferFileData(SearchMyPupilListViewModel model)
    {
        SetSelections(
        model.PageLearnerNumbers.Split(','),
        model.SelectedPupil);

        var selectedPupils = GetSelected(model.Upn.FormatLearnerNumbers());

        if (selectedPupils == null || selectedPupils?.Count == 0)
        {
            model.NoPupilSelected = true;
            return await MyPupilList(model, model.PageNumber, true);
        }

        if (selectedPupils.Count > _appSettings.CommonTransferFileUPNLimit)
        {
            model.ErrorDetails = DownloadErrorMessages.UPNLimitExceeded;
            return await MyPupilList(model, model.PageNumber, true);
        }

        if (PupilHelper.CheckIfStarredPupil(selectedPupils.ToArray()) && !model.StarredPupilConfirmationViewModel.ConfirmationGiven)
        {
            model.StarredPupilConfirmationViewModel.SelectedPupil = string.Join(',', selectedPupils);
            PopulateConfirmationViewModel(model.StarredPupilConfirmationViewModel, model);
            model.StarredPupilConfirmationViewModel.DownloadType = DownloadType.CTF;
            return ConfirmationForStarredPupil(model.StarredPupilConfirmationViewModel);
        }

        return await DownloadCommonTransferFileData(model, selectedPupils.ToArray());
    }

    private async Task<IActionResult> DownloadCommonTransferFileData(SearchMyPupilListViewModel model, string[] selectedPupils)
    {
        var downloadFile = await _ctfService.GetCommonTransferFile(selectedPupils,
            model.Upn.FormatLearnerNumbers(),
                                                                User.GetLocalAuthorityNumberForEstablishment(),
                                                                User.GetEstablishmentNumber(),
                                                                User.IsOrganisationEstablishment(),
                                                                AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId()),
                                                                ReturnRoute.MyPupilList);

        if (downloadFile.Bytes != null)
        {
            return SearchDownloadHelper.DownloadFile(downloadFile);
        }
        else
        {
            model.ErrorDetails = DownloadErrorMessages.NoDataForSelectedPupils;
        }

        return await MyPupilList(model, model.PageNumber, true, true);
    }

    [Route(Routes.PupilPremium.LearnerNumberDownloadRequest)]
    [HttpPost]
    public async Task<IActionResult> ToDownloadSelectedPupilPremiumDataUPN(SearchMyPupilListViewModel model)
    {
        SetSelections(
        model.PageLearnerNumbers.Split(','),
        model.SelectedPupil);

        var selectedPupils = GetSelected(model.Upn.FormatLearnerNumbers());

        if (selectedPupils == null || selectedPupils?.Count == 0)
        {
            model.NoPupilSelected = true;
            return await MyPupilList(model, model.PageNumber, true);
        }

        if (PupilHelper.CheckIfStarredPupil(selectedPupils.ToArray()) && !model.StarredPupilConfirmationViewModel.ConfirmationGiven)
        {
            model.StarredPupilConfirmationViewModel.SelectedPupil = string.Join(',', selectedPupils);
            PopulateConfirmationViewModel(model.StarredPupilConfirmationViewModel, model);
            model.StarredPupilConfirmationViewModel.DownloadType = DownloadType.PupilPremium;
            return ConfirmationForStarredPupil(model.StarredPupilConfirmationViewModel);
        }

        return await DownloadPupilPremiumData(model, selectedPupils.ToArray());
    }

    private async Task<IActionResult> DownloadPupilPremiumData(SearchMyPupilListViewModel model, string[] selectedPupils)
    {
        var userOrganisation = new UserOrganisation
        {
            IsAdmin = User.IsAdmin(),
            IsEstablishment = User.IsOrganisationEstablishment(),
            IsLa = User.IsOrganisationLocalAuthority(),
            IsMAT = User.IsOrganisationMultiAcademyTrust(),
            IsSAT = User.IsOrganisationSingleAcademyTrust()
        };

        var downloadFile = await _downloadService.GetPupilPremiumCSVFile(selectedPupils, selectedPupils,
            true, AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId()), ReturnRoute.MyPupilList, userOrganisation).ConfigureAwait(false);

        if (downloadFile == null)
        {
            return RedirectToAction(Routes.Application.Error, Routes.Application.Home);
        }

        if (downloadFile.Bytes != null)
        {
            return SearchDownloadHelper.DownloadFile(downloadFile);
        }
        else
        {
            model.ErrorDetails = DownloadErrorMessages.NoDataForSelectedPupils;
        }

        return await MyPupilList(model, model.PageNumber, true, true);
    }

    [Route(Routes.NationalPupilDatabase.LearnerNumberDownloadRequest)]
    [HttpPost]
    public async Task<IActionResult> ToDownloadSelectedNPDDataUPN(SearchMyPupilListViewModel model)
    {
        SetSelections(
            model.PageLearnerNumbers.Split(','),
            model.SelectedPupil);

        var selectedPupils = GetSelected(model.Upn.FormatLearnerNumbers());

        if (selectedPupils == null || selectedPupils?.Count == 0)
        {
            model.NoPupilSelected = true;
            return await MyPupilList(model, model.PageNumber, true);
        }

        if (PupilHelper.CheckIfStarredPupil(selectedPupils.ToArray()) && !model.StarredPupilConfirmationViewModel.ConfirmationGiven)
        {
            model.StarredPupilConfirmationViewModel.SelectedPupil = string.Join(',', selectedPupils);
            PopulateConfirmationViewModel(model.StarredPupilConfirmationViewModel, model);
            model.StarredPupilConfirmationViewModel.DownloadType = DownloadType.NPD;
            return ConfirmationForStarredPupil(model.StarredPupilConfirmationViewModel);
        }

        var joinedSelectedPupils = String.Join(',', selectedPupils);
        return await DownloadSelectedNationalPupilDatabaseData(joinedSelectedPupils, model.Upn, selectedPupils.Count);
    }

    [Route(Routes.DownloadSelectedNationalPupilDatabaseData)]
    [HttpPost]
    public async Task<IActionResult> DownloadSelectedNationalPupilDatabaseData(
       string selectedPupilsJoined,
       string learnerNumber,
       int selectedPupilsCount)
    {
        var searchDownloadViewModel = new LearnerDownloadViewModel
        {
            SelectedPupils = selectedPupilsJoined,
            LearnerNumber = learnerNumber,
            ErrorDetails = (string)TempData["ErrorDetails"],
            SelectedPupilsCount = selectedPupilsCount,
            DownloadFileType = DownloadFileType.CSV,
            ShowTABDownloadType = true
        };

        SearchDownloadHelper.AddDownloadDataTypes(searchDownloadViewModel, User, User.GetOrganisationLowAge(), User.GetOrganisationHighAge(), User.IsOrganisationLocalAuthority(), User.IsOrganisationAllAges());

        LearnerNumberSearchViewModel.MaximumLearnerNumbersPerSearch = _appSettings.MaximumUPNsPerSearch;
        ModelState.Clear();
        searchDownloadViewModel.NumberSearchViewModel.LearnerNumber = selectedPupilsJoined.Replace(",", "\r\n");
        searchDownloadViewModel.SearchAction = SearchAction;
        searchDownloadViewModel.DownloadRoute = Routes.NationalPupilDatabase.LearnerNumberDownloadFile;

        var selectedPupils = selectedPupilsJoined.Split(',');
        if (selectedPupils.Length < _appSettings.DownloadOptionsCheckLimit)
        {
            var downloadTypeArray = searchDownloadViewModel.SearchDownloadDatatypes.Select(d => d.Value).ToArray();
            var disabledTypes = await _downloadService.CheckForNoDataAvailable(selectedPupils, selectedPupils, downloadTypeArray, AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId())).ConfigureAwait(false);
            SearchDownloadHelper.DisableDownloadDataTypes(searchDownloadViewModel, disabledTypes);
        }

        searchDownloadViewModel.SearchResultPageHeading = PageHeading;
        return View(Global.MPLDownloadNPDOptionsView, searchDownloadViewModel);
    }

    [Route(Routes.NationalPupilDatabase.LearnerNumberDownloadFile)]
    [HttpPost]
    public async Task<IActionResult> DownloadSelectedNationalPupilDatabaseData(LearnerDownloadViewModel model)
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
                var downloadFile = model.DownloadFileType == DownloadFileType.CSV ?
                    await _downloadService.GetCSVFile(selectedPupils, selectedPupils, model.SelectedDownloadOptions, true, AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId()), ReturnRoute.NationalPupilDatabase).ConfigureAwait(false) :
                    await _downloadService.GetTABFile(selectedPupils, selectedPupils, model.SelectedDownloadOptions, true, AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId()), ReturnRoute.NationalPupilDatabase).ConfigureAwait(false);

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

            return await DownloadSelectedNationalPupilDatabaseData(model.SelectedPupils, model.LearnerNumber, model.SelectedPupilsCount);
        }

        return RedirectToAction(Global.MyPupilListAction, Global.MyPupilListControllerName);
    }

    /// <summary>
    /// Action to download a file of optional type, having passed confirmation of out of range/area of pupil. Also updates MPL to remove flag from appropriate records.
    /// </summary>
    /// <param name="model">Starred Pupil Confirmation Model</param>
    /// <returns>CTF, NPD, or PupilPremium file unless Model Error, in which case return to provide confirmation</returns>
    [HttpPost]
    [Route(Routes.SearchMyPupilList.DownloadNonUPNConfirmationReturn)]
    public async Task<IActionResult> DownloadFileConfirmationReturn(StarredPupilConfirmationViewModel model)
    {
        model.ConfirmationError = !model.ConfirmationGiven;
        PopulateConfirmationViewModel(model);

        if (model.ConfirmationGiven)
        {
            IEnumerable<string> joinedLearnerNumbers = model.SelectedPupil.Split(",");
            if (PupilHelper.CheckIfStarredPupil(model.SelectedPupil))
            {
                model.SelectedPupil = string.Join(",", RbacHelper.DecryptUpnCollection(joinedLearnerNumbers));
            }

            await _mplService.UpdatePupilMasks(model.SelectedPupil.Split(",").ToList(), false, User.GetUserId(), AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId()));

            switch (model.DownloadType)
            {
                case DownloadType.CTF: return await DownloadCommonTransferFileData(new SearchMyPupilListViewModel() { SelectedPupil = model.SelectedPupil.Split(",").ToList(), Upn = model.LearnerNumbers }, model.SelectedPupil.Split(",").ToArray());
                case DownloadType.NPD: return await DownloadSelectedNationalPupilDatabaseData(model.SelectedPupil, model.LearnerNumbers, joinedLearnerNumbers.Count());
                case DownloadType.PupilPremium: return await DownloadPupilPremiumData(new SearchMyPupilListViewModel() { SelectedPupil = model.SelectedPupil.Split(",").ToList(), Upn = model.LearnerNumbers }, model.SelectedPupil.Split(",").ToArray());
            }
        }

        return ConfirmationForStarredPupil(model);
    }

    [Route(Routes.SearchMyPupilList.DownloadCancellationReturn)]
    [HttpPost]
    public async Task<IActionResult> DownloadCancellationReturn(LearnerTextSearchViewModel model)
    {
        return await Search(true);
    }


    /// <summary>
    /// Action used to provide prompt around download of starred UPN
    /// </summary>
    /// <param name="model">Generic model for starred pupil confirmation</param>
    /// <returns>View of page with checkbox confirming acceptance</returns>
    [NonAction]
    public IActionResult ConfirmationForStarredPupil(StarredPupilConfirmationViewModel model)
    {
        return View(Global.StarredPupilConfirmationView, model);
    }


    [NonAction]
    private async Task<IActionResult> Search(bool returnToMPL)
    {
        LearnerNumberSearchViewModel.MaximumLearnerNumbersPerSearch = _appSettings.MaximumUPNsPerSearch;
        var model = new SearchMyPupilListViewModel();

        PopulatePageText(model);
        PopulateNavigation(model);
        SetModelApplicationLabels(model);
        SearchMyPupilListViewModel.MaximumUPNsPerSearch = _appSettings.MaximumUPNsPerSearch;

        var learnerList = await _mplService.GetMyPupilListLearnerNumbers(User.GetUserId());

        if (returnToMPL && this.HttpContext.Session.Keys.Contains(SortFieldSessionKey) && this.HttpContext.Session.Keys.Contains(SortDirectionSessionKey))
        {
            model.SortField = this.HttpContext.Session.GetString(SortFieldSessionKey);
            model.SortDirection = this.HttpContext.Session.GetString(SortDirectionSessionKey);
        }

        model.Upn = GetMyPupilListStringSeparatedBy(myPupilList: learnerList, separator: "\n");
        model.MyPupilList = learnerList.ToList();

        model = await GetPupilsForSearchBuilder(model, 0, !returnToMPL).ConfigureAwait(false);
        model.PageNumber = 0;
        model.PageSize = PAGESIZE;

        return View(Routes.SearchMyPupilList.MyPupilListView, model);
    }

    [NonAction]
    public async Task<IActionResult> Search(SearchMyPupilListViewModel model, int pageNumber, bool hasQueryItem = false, bool calledByController = false, bool failedDownload = false)
    {
        PopulatePageText(model);
        PopulateNavigation(model);
        SetModelApplicationLabels(model);
        SearchMyPupilListViewModel.MaximumUPNsPerSearch = _appSettings.MaximumUPNsPerSearch;

        var notPaged = !hasQueryItem && !calledByController;
        var allSelected = false;

        model.SearchBoxErrorMessage = ModelState.IsValid is false ? PupilHelper.GenerateValidationMessageUpnSearch(ModelState) : null;

        model.Upn = SecurityHelper.SanitizeText(model.Upn);

        if (ModelState.IsValid)
        {
            if (!String.IsNullOrEmpty(model.SelectAllNoJsChecked))
            {
                var selectAll = Convert.ToBoolean(model.SelectAllNoJsChecked);
                var upns = model.Upn.FormatLearnerNumbers();
                if (selectAll)
                {
                    _selectionManager.AddAll(upns);
                    model.ToggleSelectAll = true;
                }
                else
                {
                    _selectionManager.RemoveAll(upns);
                    model.ToggleSelectAll = false;
                }

                model.SelectAllNoJsChecked = null;
                allSelected = true;
            }

            if (!notPaged && !allSelected && !failedDownload)
            {
                SetSelections(
                    model.PageLearnerNumbers.Split(','),
                    model.SelectedPupil);
            }

            model = await GetPupilsForSearchBuilder(
                model,
                pageNumber,
                notPaged).ConfigureAwait(false);
            model.PageNumber = pageNumber;
            model.PageSize = PAGESIZE;
        }

        this.HttpContext.Session.SetString(SortFieldSessionKey, model.SortField ?? "");
        this.HttpContext.Session.SetString(SortDirectionSessionKey, model.SortDirection ?? "");

        return View(Routes.SearchMyPupilList.MyPupilListView, model);
    }

    private async Task<SearchMyPupilListViewModel> GetPupilsForSearchBuilder(
        SearchMyPupilListViewModel model,
        int pageNumber,
        bool first)
    {
        if (string.IsNullOrEmpty(model.Upn))
        {
            model.NoPupil = true;
            return model;
        }

        string[] upnArray = model.Upn.FormatLearnerNumbers();

        IEnumerable<MyPupilListItem> learnerList = await GetLearnerListForCurrentUser();
        string learnerListSearchText = GetMyPupilListStringSeparatedBy(myPupilList: learnerList, separator: ",");

        //need pp result & npd result - get all and take pagesize at a time
        var ppResult = await _paginatedSearch.GetPage(
            learnerListSearchText,
            null,
           _appSettings.MaximumULNsPerSearch,
            0,
            AzureSearchIndexType.PupilPremium,
            AzureSearchQueryType.Numbers,
            AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId()),
            model.SortField ?? "",
            model.SortDirection ?? "");

        var npdResult = await _paginatedSearch.GetPage(
           learnerListSearchText,
           null,
            _appSettings.MaximumULNsPerSearch,
           0,
           AzureSearchIndexType.NPD,
           AzureSearchQueryType.Numbers,
           AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId()),
            model.SortField ?? "",
            model.SortDirection ?? ""
           );

        var learners = npdResult.Learners.Union(ppResult.Learners).ToList();

        if (!learners.Any())
        {
            model.NoPupil = true;
            return model;
        }

        model.MyPupilList = learnerList.ToList();

        var whiteListUPNs = model.MyPupilList.Where(x => x.IsMasked == false).Select(x => x.PupilId);

        //set default LearnerNumberId
        learners.ForEach(x => x.LearnerNumberId = x.LearnerNumber);
        var unionLearnerNumbers = ProcessStarredPupils(learners, whiteListUPNs);

        model.Upn = string.Join("\n", unionLearnerNumbers);

        // ensure that the selections are set appropriately
        if (first)
        {
            var decryptedLearnerNumbers = RbacHelper.DecryptUpnCollection(unionLearnerNumbers);
            var missing = upnArray.Except(decryptedLearnerNumbers).ToList();
            this.HttpContext.Session.SetString(MISSING_LEARNER_NUMBERS_KEY, JsonConvert.SerializeObject(missing));
            _selectionManager.RemoveAll(unionLearnerNumbers);
            model.ToggleSelectAll = false;
        }
        else
        {
            var selected = GetSelected(unionLearnerNumbers.ToArray());

            foreach (var learner in learners)
            {
                learner.Selected = selected.Contains(learner.LearnerNumber.Equals(Global.UpnMask) ? learner.LearnerNumberId : learner.LearnerNumber);
            }

            if (selected.Any())
            {
                model.ToggleSelectAll = true;
            }
            else
            {
                model.ToggleSelectAll = false;
            }
        }

        PopulateLearners(learners, model, ppResult.Learners, pageNumber);

        return model;
    }

    /// <summary>
    /// Populates model Learners list with updated Invalid and PupilPremium properties, chunked and sorted.
    /// </summary>
    /// <param name="learners">List of learners to process</param>
    /// <param name="model">Model to update</param>
    /// <param name="ppLearners">List of pupil premium result learners to set PupilPremium property</param>
    /// <param name="pageNumber">Page number to chunk results</param>
    /// <returns>Updated SearchMyPupilListViewModel</returns>
    public SearchMyPupilListViewModel PopulateLearners(IEnumerable<Learner> learners, SearchMyPupilListViewModel model, List<Learner> ppLearners, int pageNumber)
    {
        foreach (var learner in learners)
        {
            var isMasked = learner.LearnerNumber.Equals(Global.UpnMask);
            SetPupilPremiumLabel(learner, ppLearners, isMasked);
            SetInvalid(learner, model, isMasked);
        }

        learners = learners.Except(model.Invalid);

        if (!string.IsNullOrWhiteSpace(model.SortField))
        {
            learners = SetSort(learners, model.SortField, model.SortDirection);
        }

        model.Total = learners.Count();
        model.Learners = learners.Skip(PAGESIZE * pageNumber).Take(PAGESIZE);
        model.PageLearnerNumbers = PopulatePageLearnerNumbers(model.Learners, model.Invalid);

        return model;
    }

    /// <summary>
    /// Populates PupilPremium label property of Learner is present in pupil premium results.
    /// </summary>
    /// <param name="learner">Learner to process</param>
    /// <param name="ppLearners">List of pupil premium result learners to set PupilPremium property</param>
    /// <param name="isMasked">Bool to show if learner's learnernumber is masked</param>
    private void SetPupilPremiumLabel(Learner learner, List<Learner> ppLearners, bool isMasked)
    {
        var itemExists = ppLearners.Exists(x => isMasked ? (x.LearnerNumber == RbacHelper.DecryptUpn(learner.LearnerNumberId)) : (x.LearnerNumber == learner.LearnerNumber));

        learner.PupilPremium = itemExists ? "Yes" : "No";
    }

    /// <summary>
    /// Checks if learner number is valid and if so, adds to Invalid list of model.
    /// </summary>
    /// <param name="learner">Learner to process</param>
    /// <param name="model">Model to update Invalid property if relevant</param>
    /// <param name="isMasked">Bool to show if learner's learnernumber is masked</param>
    private void SetInvalid(Learner learner, SearchMyPupilListViewModel model, bool isMasked)
    {
        bool isValid = ValidationHelper.IsValidUpn(isMasked ? RbacHelper.DecryptUpn(learner.LearnerNumberId) : learner.LearnerNumber);

        if (!isValid)
        {
            model.Invalid.Add(learner);
        }
    }

    private SearchMyPupilListViewModel PopulatePageText(SearchMyPupilListViewModel model)
    {
        model.PageHeading = PageHeading;
        model.LearnerNumberLabel = Global.LearnerNumberLabel;
        return model;
    }

    private SearchMyPupilListViewModel PopulateNavigation(SearchMyPupilListViewModel model)
    {
        model.ShowLocalAuthority = _appSettings.UseLAColumn;
        model.DownloadLinksPartial = DownloadLinksPartial;
        model.SearchAction = SearchAction;
        return model;
    }

    /// <summary>
    /// Applies RBAC rules to learner list, masking learner numbers if required.
    /// </summary>
    /// <param name="learners">List of learners to process</param>
    /// /// <param name="whiteList">List of UPNs that are to be excluded from RBAC</param>
    ///  <returns>Combined list of learner numbers including encrypted if present </returns>

    private IEnumerable<string> ProcessStarredPupils(IEnumerable<Learner> learners, IEnumerable<string> whiteList)
    {
        var isAdmin = User.IsAdmin();
        var lowAge = User.GetOrganisationLowAge();
        var highAge = User.GetOrganisationHighAge();

        if (!isAdmin)
        {
            var learnersExemptFromMask = learners.Where(x => whiteList.Contains(x.LearnerNumber));
            var learnersNotExempt = learners.Where(x => !whiteList.Contains(x.LearnerNumber));

            learnersNotExempt = RbacHelper.CheckRbacRulesGeneric<Learner>(learnersNotExempt.ToList(), lowAge, highAge);

            learners = learnersNotExempt.Union(learnersExemptFromMask);
        }

        return learners.Select(l => l.LearnerNumberId);
    }

    private HashSet<string> GetSelected(string[] available)
    {
        // ensure we remove the missing items
        var missing = JsonConvert.DeserializeObject<List<string>>(this.HttpContext.Session.GetString(MISSING_LEARNER_NUMBERS_KEY));

        if (missing != null)
        {
            var actuallyAvailable = available.Except(missing).ToArray();
            return _selectionManager.GetSelected(actuallyAvailable);
        }

        return _selectionManager.GetSelected(available);
    }

    private void SetSelections(IEnumerable<string> available, IEnumerable<string> selected)
    {
        IEnumerable<string> toAdd;
        IEnumerable<string> toRemove;
        if (selected != null)
        {
            toAdd = selected;
            toRemove = available.Except(selected);
        }
        else
        {
            toAdd = new List<string>();
            toRemove = available; // nothing selected, remove them all.
        }

        _selectionManager.AddAll(toAdd);
        _selectionManager.RemoveAll(toRemove);
    }

    private void PopulateConfirmationViewModel(StarredPupilConfirmationViewModel model, SearchMyPupilListViewModel mplModel = null)
    {
        model.ConfirmationReturnController = Global.MyPupilListControllerName;
        model.ConfirmationReturnAction = Global.MyPupilListDownloadConfirmationReturnAction;
        model.CancelReturnController = Global.MyPupilListControllerName;
        model.CancelReturnAction = Global.MyPupilListDownloadCancellationReturnAction;
        if (mplModel != null)
        {
            model.LearnerNumbers = mplModel.Upn;
        }
    }

    /// <summary>
    /// Collates all learner numbers present on page, including invalid and masked.
    /// </summary>
    /// <param name="learners">List of learners to process</param>
    /// <param name="invalid">List of invalid learners to process</param>        ///
    ///  <returns>Combined list of page learner numbers including invalid and encrypted if present </returns>
    private string PopulatePageLearnerNumbers(IEnumerable<Learner> learners, IEnumerable<Learner> invalid)
    {
        var pageLearnerNumbers = learners.Select(l => l.LearnerNumber).Where(l => !l.Equals(Global.UpnMask));
        var pageLearnerNumberIds = from learner in learners
                                   where !string.IsNullOrEmpty(learner.LearnerNumberId)
                                   select learner.LearnerNumberId;
        var pageInvalidLearnerNumbers = invalid.Select(l => l.LearnerNumber).Where(l => !l.Equals(Global.UpnMask));
        var pageInvalidLearnerNumberIds = from learner in invalid
                                          where !string.IsNullOrEmpty(learner.LearnerNumberId)
                                          select learner.LearnerNumberId;
        var pageUnionLearnerNumbers = pageLearnerNumbers.Union(pageLearnerNumberIds).Union(pageInvalidLearnerNumbers).Union(pageInvalidLearnerNumberIds);
        return string.Join(',', pageUnionLearnerNumbers);
    }

    private IEnumerable<Learner> SetSort(IEnumerable<Learner> learners, string sortField, string sortDirection)
    {
        PropertyInfo prop = typeof(Learner).GetProperty(sortField);

        return sortDirection switch
        {
            AzureSearchSortDirections.Ascending => learners.OrderBy(x => prop.GetValue(x)),
            AzureSearchSortDirections.Descending => learners.OrderByDescending(x => prop.GetValue(x)),
            _ => learners
        };
    }

    private float NumberOfPagesRemainingAfterSelectedPupilsRemoved(
         float totalNumberOfPupils,
         float totalNumberOfPupilsToRemove) =>
         (totalNumberOfPupils - totalNumberOfPupilsToRemove) / PAGESIZE;

    /// <summary>
    /// Ensures that a page which is partially populated is calculated
    /// in the whole page count. i.e. a value of 2.3 demotes 3 pages.
    /// </summary>
    private void SetRevisedCurrentPageNumber(
        float numberOfPagesRemainingAfterSelectedPupilsRemoved,
        SearchMyPupilListViewModel model)
    {
        double pageUpperBoundey =
            Math.Ceiling(numberOfPagesRemainingAfterSelectedPupilsRemoved);
        model.PageNumber = (int)pageUpperBoundey - 1;
    }

    private Task<IEnumerable<MyPupilListItem>> GetLearnerListForCurrentUser() =>
        _mplService.GetMyPupilListLearnerNumbers(User.GetUserId());

    private string GetMyPupilListStringSeparatedBy(
        IEnumerable<MyPupilListItem> myPupilList, string separator) =>
            string.Join(separator, myPupilList.Select(myPupilListItem => myPupilListItem.PupilId));

    private void SetModelApplicationLabels(SearchMyPupilListViewModel model)
    {
        model.DownloadSelectedASCTFLink = ApplicationLabel.DownloadSelectedAsCtfLink;
        model.RemoveSelectedToMyPupilListLink = ApplicationLabel.RemoveSelectedToMyPupilListLink;
        model.DownloadSelectedNPDDataLink = ApplicationLabel.DownloadSelectedNationalPupilDatabaseDataLink;
        model.DownloadSelectedPupilPremiumDataLink = ApplicationLabel.DownloadSelectedPupilPremiumDataLink;
    }
}
