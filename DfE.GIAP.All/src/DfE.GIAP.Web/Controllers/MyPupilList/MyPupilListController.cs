using System.Reflection;
using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Common.Helpers;
using DfE.GIAP.Common.Helpers.Rbac;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Request;
using DfE.GIAP.Core.MyPupils.Domain.Aggregate;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Domain.Models.MPL;
using DfE.GIAP.Domain.Models.User;
using DfE.GIAP.Domain.Search.Learner;
using DfE.GIAP.Service.Common;
using DfE.GIAP.Service.Download;
using DfE.GIAP.Service.Download.CTF;
using DfE.GIAP.Service.MPL;
using DfE.GIAP.Service.Search;
using DfE.GIAP.Web.Authorisation;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Helpers.Search;
using DfE.GIAP.Web.Helpers.SearchDownload;
using DfE.GIAP.Web.Helpers.SelectionManager;
using DfE.GIAP.Web.ViewModels;
using DfE.GIAP.Web.ViewModels.Search;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace DfE.GIAP.Web.Controllers.MyPupilList;

[Route(Routes.Application.MyPupilList)]
public class MyPupilListController : Controller
{
    public const int PAGESIZE = 20;
    public const string MISSING_LEARNER_NUMBERS_KEY = "missingLearnerNumbers";

    private readonly ILogger<MyPupilListController> _logger;
    private readonly ICommonService _commonService;
    private readonly IUseCase<GetMyPupilsRequest, GetMyPupilsResponse> _getMyPupilsUseCase;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IPaginatedSearchService _paginatedSearch;
    private readonly ISelectionManager _selectionManager;
    private readonly IMyPupilListService _mplService;
    private readonly IDownloadCommonTransferFileService _ctfService;
    private readonly IDownloadService _downloadService;
    private readonly AzureAppSettings _appSettings;

    //public string SortFieldSessionKey = "SearchMPL_SortField";
    //public string SortDirectionSessionKey = "SearchMPL_SortDirection";

    public MyPupilListController(
        ILogger<MyPupilListController> logger,
        IPaginatedSearchService paginatedSearch,
        IMyPupilListService mplService,
        ISelectionManager selectionManager,
        IDownloadCommonTransferFileService ctfService,
        IDownloadService downloadService,
        ICommonService commonService,
        IOptions<AzureAppSettings> azureAppSettings,
        IUseCase<GetMyPupilsRequest, GetMyPupilsResponse> getMyPupilsUseCase,
        IHttpContextAccessor httpContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;

        ArgumentNullException.ThrowIfNull(commonService);
        _commonService = commonService;

        ArgumentNullException.ThrowIfNull(getMyPupilsUseCase);
        _getMyPupilsUseCase = getMyPupilsUseCase;

        ArgumentNullException.ThrowIfNull(httpContextAccessor);
        _httpContextAccessor = httpContextAccessor;

        ArgumentNullException.ThrowIfNull(paginatedSearch);
        _paginatedSearch = paginatedSearch;

        ArgumentNullException.ThrowIfNull(selectionManager);
        _selectionManager = selectionManager;

        ArgumentNullException.ThrowIfNull(mplService);
        _mplService = mplService;

        ArgumentNullException.ThrowIfNull(ctfService);
        _ctfService = ctfService;

        ArgumentNullException.ThrowIfNull(azureAppSettings);
        ArgumentNullException.ThrowIfNull(azureAppSettings.Value);
        _appSettings = azureAppSettings.Value;

        ArgumentNullException.ThrowIfNull(downloadService);
        _downloadService = downloadService;
    }

    public sealed class PupilPresentatationModel
    {
        public PupilPresentatationModel(PupilDto dto)
        {
            Id = dto.Id;
            UniquePupilNumber = dto.UniquePupilNumber;
            Forename = dto.Forename;
            Surname = dto.Surname;
            DateOfBirth = dto.DateOfBirth;
            PupilPremiumLabel = dto.IsPupilPremium ? "Yes" : "No";
            Sex = dto.Sex;
            LocalAuthorityCode = dto.LocalAuthorityCode.ToString();
        }

        public string Id { get; }
        public string UniquePupilNumber { get; }
        public string Forename { get; }
        public string Surname { get; }
        public string DateOfBirth { get; }
        public string PupilPremiumLabel { get; }
        public string Sex { get; }
        public string LocalAuthorityCode { get; }
    }

    public record MyPupilsErrorModel(string Message);

    public sealed class MyPupilsViewModel
    {
        private readonly MyPupilsErrorModel? _error;
        public MyPupilsViewModel(
            IEnumerable<PupilPresentatationModel> pupils,
            MyPupilsErrorModel? error = null)
        {
            Pupils = pupils;
            _error = error;
        }

        public string PageHeading => "My pupil list";

        public IEnumerable<PupilPresentatationModel> Pupils { get; } = [];
        public IEnumerable<Learner> InvalidUpns => [];

        public bool IsPreviousPageAvailable => PageNumber > 1;
        public bool IsMorePageAvailable => Pupils.Count() == PageSize;
        public bool NoPupils => Pupils.Count() == 0;

        public int PageNumber { get; set; } = 1;
        public int PageSize => 20;
        public string SortField { get; set; } = string.Empty;
        public string SortDirection { get; set; } = string.Empty;

        public string UniquePupilNumberLabel => "UPN";
        public string DownloadLinksPartial => "~/Views/MyPupilList/_MyPupilListDownloadLinks.cshtml";
        public string FormAction => "MyPupilList";
        public int MaximumUPNsPerSearch => 4000;

        public bool isRemovedSuccessful => false;
        public bool NoPupilSelected => false;
        public bool ToggleSelectAll => false;

        public string ErrorMessage => _error?.Message ?? string.Empty;
        public bool IsError => _error is not null;
    }

    public sealed class PaginatedMyPupilsRequestDto
    {
        public string SortField { get; set; } = string.Empty;
        public string SortDirection { get; set; } = string.Empty;
        public int PageNumber { get; set; } = 1;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        _logger.LogInformation("My pupil list GET method is called");

        GetMyPupilsRequest request = new(
            new HttpContextAuthorisationContextAdaptor(_httpContextAccessor));

        GetMyPupilsResponse getMyPupilsResponse = await _getMyPupilsUseCase.HandleRequestAsync(request);

        IEnumerable<PupilPresentatationModel> pupils = getMyPupilsResponse.Pupils.Select((t) => new PupilPresentatationModel(t));

        MyPupilsViewModel model = new(pupils);

        return View(Routes.MyPupilList.MyPupilListView, model);
    }

    [HttpPost]
    public async Task<IActionResult> MyPupilList(
        MyPupilListViewModel model, // TODO take in PaginatedMyPupilsRequestDt and hydrate MyPupilsViewModel. WAITING on calls from Remove / Download
        [FromQuery] int pageNumber)
        //,
        //bool calledByController = false,
        //bool failedDownload = false)
    {
        _logger.LogInformation("My pupil list UPN POST method called");

        if (!ModelState.IsValid)
        {
            MyPupilsErrorModel error = new(PupilHelper.GenerateValidationMessageUpnSearch(ModelState));
            MyPupilsViewModel outputModelOnError = new([], error); // Should this attempt to use existing pupils if they exist or just return empty?
            return View(Routes.MyPupilList.MyPupilListView, outputModelOnError);
        }

        OrderPupilsBy order = new(
            field: model.SortField,
            direction: model.SortDirection switch
        {
            "asc" => SortDirection.Ascending,
            "desc" => SortDirection.Descending,
            _ => SortDirection.Default
        });

        GetMyPupilsRequest request = new(
            new HttpContextAuthorisationContextAdaptor(_httpContextAccessor),
            new MyPupilsQueryOptions(
                order,
                PageNumber.Page(pageNumber)));

        GetMyPupilsResponse getMyPupilsResponse = await _getMyPupilsUseCase.HandleRequestAsync(request);

        IEnumerable<PupilPresentatationModel> pupils = getMyPupilsResponse.Pupils.Select((t) => new PupilPresentatationModel(t));

        MyPupilsViewModel presentationModel = new(pupils)
        {
            PageNumber = pageNumber,
            SortField = model.SortField,
            SortDirection = model.SortDirection
        };

        return View(Routes.MyPupilList.MyPupilListView, presentationModel);

        /***** OLD IMPLEMENTATION BELOW */
        // There's a select all flag which is passed, not sure what relevance that has for sorting / paging though, only relevant for Removal / Downloads.

        //PopulatePageText(model);
        //PopulateNavigation(model);
        //SetModelApplicationLabels(model);
        ////MyPupilListViewModel.MaximumUPNsPerSearch = _appSettings.MaximumUPNsPerSearch; // TODO guard

        //var notPaged = !ControllerContext.HttpContext.Request.Query.ContainsKey("pageNumber") && !calledByController;
        //var allSelected = false;

        //model.Upn = SecurityHelper.SanitizeText(model.Upn);

        //if (ModelState.IsValid)
        //{
        //    if (!string.IsNullOrEmpty(model.SelectAllNoJsChecked))
        //    {
        //        var selectAll = Convert.ToBoolean(model.SelectAllNoJsChecked);
        //        var upns = model.Upn.FormatLearnerNumbers();
        //        if (selectAll)
        //        {
        //            _selectionManager.AddAll(upns);
        //            model.ToggleSelectAll = true;
        //        }
        //        else
        //        {
        //            _selectionManager.RemoveAll(upns);
        //            model.ToggleSelectAll = false;
        //        }

        //        model.SelectAllNoJsChecked = null;
        //        allSelected = true;
        //    }

        //    if (!notPaged && !allSelected && !failedDownload)
        //    {
        //        SetSelections(model.PageLearnerNumbers.Split(','), model.SelectedPupil);
        //    }

        //    model = await GetPupilsForSearchBuilder(model, pageNumber, notPaged);
        //    model.PageNumber = pageNumber;
        //    model.PageSize = PAGESIZE;
        //}

        //HttpContext.Session.SetString(SortFieldSessionKey, model.SortField ?? "");
        //HttpContext.Session.SetString(SortDirectionSessionKey, model.SortDirection ?? "");
    }

    [HttpPost]
    [Route(Routes.MyPupilList.RemoveSelected)]
    public async Task<IActionResult> RemoveSelected(MyPupilListViewModel model)
    {
        _logger.LogInformation("Remove from my pupil list POST method is called");

        SetSelections(model.PageLearnerNumbers.Split(','), model.SelectedPupil);

        HashSet<string> selectedPupils = GetSelected(model.Upn.FormatLearnerNumbers());
        if (selectedPupils == null || selectedPupils.Count == 0)
        {
            model.NoPupilSelected = true;
            return await MyPupilList(model, model.PageNumber);
        }

        IEnumerable<MyPupilListItem> learnerList = await GetLearnerListForCurrentUser();
        IEnumerable<string> decryptedSelectedPupils = RbacHelper.DecryptUpnCollection(selectedPupils);
        UserProfile userProfile = new()
        {
            UserId = User.GetUserId(),
            IsPupilListUpdated = true,
            MyPupilList = learnerList.Where(x => !decryptedSelectedPupils.Contains(x.PupilId)).ToList()
        };

        await _commonService.CreateOrUpdateUserProfile(userProfile, AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId()));

        model.Upn = GetMyPupilListStringSeparatedBy(userProfile.MyPupilList, "\n");
        model.Removed = true;

        int pagesRemaining = NumberOfPagesRemainingAfterSelectedPupilsRemoved(model.Total, selectedPupils.Count);
        SetRevisedCurrentPageNumber(pagesRemaining, model);

        return (pagesRemaining > 0)
            ? await MyPupilList(model, model.PageNumber)
            : await Index();
    }

    [HttpPost]
    [Route(Routes.DownloadCommonTransferFile.DownloadCommonTransferFileAction)]
    public Task<IActionResult> ToDownloadCommonTransferFileData(MyPupilListViewModel model)
        => HandleDownloadRequest(model, DownloadType.CTF);

    [HttpPost]
    [Route(Routes.PupilPremium.LearnerNumberDownloadRequest)]
    public Task<IActionResult> ToDownloadSelectedPupilPremiumDataUPN(MyPupilListViewModel model)
        => HandleDownloadRequest(model, DownloadType.PupilPremium);

    [HttpPost]
    [Route(Routes.NationalPupilDatabase.LearnerNumberDownloadRequest)]
    public Task<IActionResult> ToDownloadSelectedNPDDataUPN(MyPupilListViewModel model)
        => HandleDownloadRequest(model, DownloadType.NPD);

    private async Task<IActionResult> HandleDownloadRequest(MyPupilListViewModel model, DownloadType downloadType)
    {
        SetSelections(model.PageLearnerNumbers.Split(','), model.SelectedPupil);
        var selectedPupils = GetSelected(model.Upn.FormatLearnerNumbers());

        if (selectedPupils == null || selectedPupils.Count == 0)
        {
            model.NoPupilSelected = true;
            return await MyPupilList(model, model.PageNumber);
        }

        if (downloadType == DownloadType.CTF && selectedPupils.Count > _appSettings.CommonTransferFileUPNLimit)
        {
            model.ErrorDetails = Messages.Downloads.Errors.UPNLimitExceeded;
            return await MyPupilList(model, model.PageNumber);
        }

        if (PupilHelper.CheckIfStarredPupil(selectedPupils.ToArray()) && !model.StarredPupilConfirmationViewModel.ConfirmationGiven)
        {
            PrepareStarredPupilConfirmation(model, selectedPupils, downloadType);
            return ConfirmationForStarredPupil(model.StarredPupilConfirmationViewModel);
        }

        return downloadType switch
        {
            DownloadType.CTF => await DownloadCommonTransferFileData(model, selectedPupils.ToArray()),
            DownloadType.PupilPremium => await DownloadPupilPremiumData(model, selectedPupils.ToArray()),
            DownloadType.NPD => await DownloadSelectedNationalPupilDatabaseData(string.Join(',', selectedPupils), model.Upn, selectedPupils.Count),
            _ => await MyPupilList(model, model.PageNumber)
        };
    }

    private void PrepareStarredPupilConfirmation(MyPupilListViewModel model, HashSet<string> selectedPupils, DownloadType downloadType)
    {
        var confirmationModel = model.StarredPupilConfirmationViewModel;
        confirmationModel.SelectedPupil = string.Join(',', selectedPupils);
        PopulateConfirmationViewModel(confirmationModel, model);
        confirmationModel.DownloadType = downloadType;
    }

    private async Task<IActionResult> DownloadCommonTransferFileData(MyPupilListViewModel model, string[] selectedPupils)
    {
        var downloadFile = await _ctfService.GetCommonTransferFile(
            selectedPupils,
            model.Upn.FormatLearnerNumbers(),
            User.GetLocalAuthorityNumberForEstablishment(),
            User.GetEstablishmentNumber(),
            User.IsOrganisationEstablishment(),
            AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId()),
            ReturnRoute.MyPupilList);

        if (downloadFile.Bytes != null)
            return SearchDownloadHelper.DownloadFile(downloadFile);

        model.ErrorDetails = Messages.Downloads.Errors.NoDataForSelectedPupils;
        return await MyPupilList(model, model.PageNumber);
    }

    private async Task<IActionResult> DownloadPupilPremiumData(MyPupilListViewModel model, string[] selectedPupils)
    {
        var userOrganisation = new UserOrganisation
        {
            IsAdmin = User.IsAdmin(),
            IsEstablishment = User.IsOrganisationEstablishment(),
            IsLa = User.IsOrganisationLocalAuthority(),
            IsMAT = User.IsOrganisationMultiAcademyTrust(),
            IsSAT = User.IsOrganisationSingleAcademyTrust()
        };

        var downloadFile = await _downloadService.GetPupilPremiumCSVFile(
            selectedPupils, selectedPupils, true,
            AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId()),
            ReturnRoute.MyPupilList, userOrganisation);

        if (downloadFile == null)
            return RedirectToAction(Routes.Application.Error, Routes.Application.Home);

        if (downloadFile.Bytes != null)
            return SearchDownloadHelper.DownloadFile(downloadFile);

        model.ErrorDetails = Messages.Downloads.Errors.NoDataForSelectedPupils;
        return await MyPupilList(model, model.PageNumber);
    }

    [HttpPost]
    [Route(Routes.DownloadSelectedNationalPupilDatabaseData)]
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

        SearchDownloadHelper.AddDownloadDataTypes(
            searchDownloadViewModel, User, User.GetOrganisationLowAge(),
            User.GetOrganisationHighAge(), User.IsOrganisationLocalAuthority(),
            User.IsOrganisationAllAges());

        LearnerNumberSearchViewModel.MaximumLearnerNumbersPerSearch = _appSettings.MaximumUPNsPerSearch;
        ModelState.Clear();
        searchDownloadViewModel.NumberSearchViewModel.LearnerNumber = selectedPupilsJoined.Replace(",", "\r\n");
        searchDownloadViewModel.SearchAction = "MyPupilList";
        searchDownloadViewModel.DownloadRoute = Routes.NationalPupilDatabase.LearnerNumberDownloadFile;

        var selectedPupils = selectedPupilsJoined.Split(',');
        if (selectedPupils.Length < _appSettings.DownloadOptionsCheckLimit)
        {
            var downloadTypeArray = searchDownloadViewModel.SearchDownloadDatatypes.Select(d => d.Value).ToArray();
            var disabledTypes = await _downloadService.CheckForNoDataAvailable(
                selectedPupils, selectedPupils, downloadTypeArray,
                AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId()));
            SearchDownloadHelper.DisableDownloadDataTypes(searchDownloadViewModel, disabledTypes);
        }

        searchDownloadViewModel.SearchResultPageHeading = ApplicationLabels.SearchMyPupilListPageHeading;
        return View(Global.MPLDownloadNPDOptionsView, searchDownloadViewModel);
    }

    [HttpPost]
    [Route(Routes.NationalPupilDatabase.LearnerNumberDownloadFile)]
    public async Task<IActionResult> DownloadSelectedNationalPupilDatabaseData(LearnerDownloadViewModel model)
    {
        if (!string.IsNullOrEmpty(model.SelectedPupils))
        {
            var selectedPupils = model.SelectedPupils.Split(',');

            if (model.SelectedDownloadOptions == null)
            {
                model.ErrorDetails = Messages.Search.Errors.SelectOneOrMoreDataTypes;
            }
            else if (model.DownloadFileType != DownloadFileType.None)
            {
                var downloadFile = model.DownloadFileType == DownloadFileType.CSV
                    ? await _downloadService.GetCSVFile(selectedPupils, selectedPupils, model.SelectedDownloadOptions, true, AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId()), ReturnRoute.NationalPupilDatabase)
                    : await _downloadService.GetTABFile(selectedPupils, selectedPupils, model.SelectedDownloadOptions, true, AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId()), ReturnRoute.NationalPupilDatabase);

                if (downloadFile == null)
                    return RedirectToAction(Routes.Application.Error, Routes.Application.Home);

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
            return await DownloadSelectedNationalPupilDatabaseData(model.SelectedPupils, model.LearnerNumber, model.SelectedPupilsCount);
        }

        return RedirectToAction(Global.MyPupilListAction, Global.MyPupilListControllerName);
    }

    [HttpPost]
    [Route(Routes.MyPupilList.DownloadNonUPNConfirmationReturn)]
    public async Task<IActionResult> DownloadFileConfirmationReturn(StarredPupilConfirmationViewModel model)
    {
        model.ConfirmationError = !model.ConfirmationGiven;
        PopulateConfirmationViewModel(model);

        if (model.ConfirmationGiven)
        {
            var joinedLearnerNumbers = model.SelectedPupil.Split(",");
            if (PupilHelper.CheckIfStarredPupil(model.SelectedPupil))
            {
                model.SelectedPupil = string.Join(",", RbacHelper.DecryptUpnCollection(joinedLearnerNumbers));
            }

            await _mplService.UpdatePupilMasks(model.SelectedPupil.Split(",").ToList(), false, User.GetUserId(), AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId()));

            return model.DownloadType switch
            {
                DownloadType.CTF => await DownloadCommonTransferFileData(new MyPupilListViewModel { SelectedPupil = model.SelectedPupil.Split(",").ToList(), Upn = model.LearnerNumbers }, model.SelectedPupil.Split(",")),
                DownloadType.NPD => await DownloadSelectedNationalPupilDatabaseData(model.SelectedPupil, model.LearnerNumbers, joinedLearnerNumbers.Length),
                DownloadType.PupilPremium => await DownloadPupilPremiumData(new MyPupilListViewModel { SelectedPupil = model.SelectedPupil.Split(",").ToList(), Upn = model.LearnerNumbers }, model.SelectedPupil.Split(",")),
                _ => ConfirmationForStarredPupil(model)
            };
        }

        return ConfirmationForStarredPupil(model);
    }

    [NonAction]
    public IActionResult ConfirmationForStarredPupil(StarredPupilConfirmationViewModel model)
        => View(Global.StarredPupilConfirmationView, model);

    // Helper methods
    private async Task<MyPupilListViewModel> GetPupilsForSearchBuilder(
        MyPupilListViewModel model,
        int pageNumber,
        bool first) //!returnToMpl ?
    {
        if (string.IsNullOrEmpty(model.Upn))
        {
            model.NoPupil = true;
            return model;
        }

        string[] upnArray = model.Upn.FormatLearnerNumbers();
        IEnumerable<MyPupilListItem> learnerList = await GetLearnerListForCurrentUser();
        string learnerListSearchText = GetMyPupilListStringSeparatedBy(learnerList, ",");

        PaginatedResponse ppResult = await _paginatedSearch.GetPage(
            learnerListSearchText, null, _appSettings.MaximumULNsPerSearch, 0,
            AzureSearchIndexType.PupilPremium, AzureSearchQueryType.Numbers,
            AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId()),
            model.SortField ?? "", model.SortDirection ?? "");

        PaginatedResponse npdResult = await _paginatedSearch.GetPage(
            learnerListSearchText, null, _appSettings.MaximumULNsPerSearch, 0,
            AzureSearchIndexType.NPD, AzureSearchQueryType.Numbers,
            AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId()),
            model.SortField ?? "", model.SortDirection ?? "");

        List<Learner> learners = npdResult.Learners.Union(ppResult.Learners).ToList();

        if (learners.Count == 0)
        {
            model.NoPupil = true;
            return model;
        }

        model.MyPupilList = learnerList.ToList();
        IEnumerable<string> whiteListUPNs = model.MyPupilList.Where(x => !x.IsMasked).Select(x => x.PupilId);

        learners.ForEach(x => x.LearnerNumberId = x.LearnerNumber);
        IEnumerable<string> unionLearnerNumbers = []; //ProcessStarredPupils(learners, whiteListUPNs);

        model.Upn = string.Join("\n", unionLearnerNumbers);

        if (first)
        {
            IEnumerable<string> decryptedLearnerNumbers = RbacHelper.DecryptUpnCollection(unionLearnerNumbers);
            List<string> missing = upnArray.Except(decryptedLearnerNumbers).ToList();
            HttpContext.Session.SetString(MISSING_LEARNER_NUMBERS_KEY, JsonConvert.SerializeObject(missing));
            _selectionManager.RemoveAll(unionLearnerNumbers);
            model.ToggleSelectAll = false;
        }
        else
        {
            HashSet<string> selected = GetSelected(unionLearnerNumbers.ToArray());
            foreach (Learner learner in learners)
            {
                learner.Selected = selected.Contains(learner.LearnerNumber.Equals(Global.UpnMask) ? learner.LearnerNumberId : learner.LearnerNumber);
            }
            model.ToggleSelectAll = selected.Any();
        }

        PopulateLearners(learners, model, ppResult.Learners, pageNumber);
        return model;
    }

    public MyPupilListViewModel PopulateLearners(IEnumerable<Learner> learners, MyPupilListViewModel model, List<Learner> ppLearners, int pageNumber)
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

    private void SetPupilPremiumLabel(Learner learner, List<Learner> ppLearners, bool isMasked)
    {
        var itemExists = ppLearners.Exists(x => isMasked
            ? (x.LearnerNumber == RbacHelper.DecodeUpn(learner.LearnerNumberId))
            : (x.LearnerNumber == learner.LearnerNumber));
        learner.PupilPremium = itemExists ? "Yes" : "No";
    }

    private void SetInvalid(Learner learner, MyPupilListViewModel model, bool isMasked)
    {
        bool isValid = ValidationHelper.IsValidUpn(isMasked ? RbacHelper.DecodeUpn(learner.LearnerNumberId) : learner.LearnerNumber);
        if (!isValid)
            model.Invalid.Add(learner);
    }

    //private MyPupilListViewModel PopulatePageText(MyPupilListViewModel model)
    //{
    //    model.PageHeading = ApplicationLabels.SearchMyPupilListPageHeading;
    //    model.LearnerNumberLabel = Global.LearnerNumberLabel;
    //    return model;
    //}

    //private MyPupilListViewModel PopulateNavigation(MyPupilListViewModel model)
    //{
    //    model.ShowLocalAuthority = _appSettings.UseLAColumn;
    //    model.DownloadLinksPartial = "~/Views/MyPupilList/_MyPupilListDownloadLinks.cshtml";
    //    model.SearchAction = "MyPupilList";
    //    return model;
    //}

    private HashSet<string> GetSelected(string[] available)
    {
        var missing = JsonConvert.DeserializeObject<List<string>>(HttpContext.Session.GetString(MISSING_LEARNER_NUMBERS_KEY));
        var actuallyAvailable = missing != null ? available.Except(missing).ToArray() : available;
        return _selectionManager.GetSelected(actuallyAvailable);
    }

    private void SetSelections(IEnumerable<string> available, IEnumerable<string> selected)
    {
        var toAdd = selected ?? Enumerable.Empty<string>();
        var toRemove = available.Except(toAdd);
        _selectionManager.AddAll(toAdd);
        _selectionManager.RemoveAll(toRemove);
    }

    private void PopulateConfirmationViewModel(StarredPupilConfirmationViewModel model, MyPupilListViewModel mplModel = null)
    {
        model.ConfirmationReturnController = Global.MyPupilListControllerName;
        model.ConfirmationReturnAction = Global.MyPupilListDownloadConfirmationReturnAction;
        model.CancelReturnController = Global.MyPupilListControllerName;
        model.CancelReturnAction = Global.MyPupilListDownloadCancellationReturnAction;
        if (mplModel != null)
            model.LearnerNumbers = mplModel.Upn;
    }

    private string PopulatePageLearnerNumbers(IEnumerable<Learner> learners, IEnumerable<Learner> invalid)
    {
        var pageLearnerNumbers = learners.Select(l => l.LearnerNumber).Where(l => !l.Equals(Global.UpnMask));
        var pageLearnerNumberIds = learners.Where(l => !string.IsNullOrEmpty(l.LearnerNumberId)).Select(l => l.LearnerNumberId);
        var pageInvalidLearnerNumbers = invalid.Select(l => l.LearnerNumber).Where(l => !l.Equals(Global.UpnMask));
        var pageInvalidLearnerNumberIds = invalid.Where(l => !string.IsNullOrEmpty(l.LearnerNumberId)).Select(l => l.LearnerNumberId);
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

    private int NumberOfPagesRemainingAfterSelectedPupilsRemoved(int total, int toRemove)
    {
        int remaining = total - toRemove;
        if (remaining <= 0)
            return 0;

        // Integer division, add one if there's a remainder
        return (remaining + PAGESIZE - 1) / PAGESIZE;
    }

    private void SetRevisedCurrentPageNumber(int pagesRemaining, MyPupilListViewModel model)
    {
        // If there are no pages, set to 0, else set to last page (zero-based)
        model.PageNumber = pagesRemaining > 0 ? pagesRemaining - 1 : 0;
    }

    private Task<IEnumerable<MyPupilListItem>> GetLearnerListForCurrentUser()
        => _mplService.GetMyPupilListLearnerNumbers(User.GetUserId());

    private string GetMyPupilListStringSeparatedBy(IEnumerable<MyPupilListItem> myPupilList, string separator)
        => string.Join(separator, myPupilList.Select(item => item.PupilId));

    private void SetModelApplicationLabels(MyPupilListViewModel model)
    {
        model.DownloadSelectedASCTFLink = ApplicationLabels.DownloadSelectedAsCtfLink;
        model.RemoveSelectedToMyPupilListLink = ApplicationLabels.RemoveSelectedToMyPupilListLink;
        model.DownloadSelectedNPDDataLink = ApplicationLabels.DownloadSelectedNationalPupilDatabaseDataLink;
        model.DownloadSelectedPupilPremiumDataLink = ApplicationLabels.DownloadSelectedPupilPremiumDataLink;
    }
}



