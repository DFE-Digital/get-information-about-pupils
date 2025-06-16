using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Constants.Messages.Common;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Common.Helpers;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Domain.Models.MPL;
using DfE.GIAP.Domain.Search.Learner;
using DfE.GIAP.Service.Content;
using DfE.GIAP.Service.MPL;
using DfE.GIAP.Service.Search;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Helpers.SelectionManager;
using DfE.GIAP.Web.ViewModels.Helper;
using DfE.GIAP.Web.ViewModels.Search;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DfE.GIAP.Web.Controllers
{
    /// <summary>
    /// Provides a base implementation for search controllers that use learner numbers (UPN/ULN)
    /// This is unit tested via its concrete implementations
    /// </summary>
    [ExcludeFromCodeCoverage]
    public abstract class BaseLearnerNumberController : Controller
    {
        public const int PAGESIZE = 20;
        public const string MISSING_LEARNER_NUMBERS_KEY = "missingLearnerNumbers";
        public const string TOTAL_SEARCH_RESULTS = "totalSearch";

        private readonly ILogger<BaseLearnerNumberController> _logger;
        private readonly IContentService _contentService;
        private readonly IPaginatedSearchService _paginatedSearch;
        protected readonly ISelectionManager _selectionManager;

        private readonly IMyPupilListService _mplService;
        private readonly AzureAppSettings _appSettings;

        #region Abstract Properties

        public abstract string PageHeading { get; }

        public abstract string SearchAction { get; }
        public abstract string FullTextLearnerSearchController { get; }
        public abstract string FullTextLearnerSearchAction { get; }
        public abstract string InvalidUPNsConfirmationAction { get; }
        public abstract string DownloadLinksPartial { get; }
        public abstract AzureSearchIndexType IndexType { get; }
        public abstract string SearchSessionKey { get; }
        public abstract string SearchSessionSortField { get; }
        public abstract string SearchSessionSortDirection { get; }

        public abstract int MyPupilListLimit { get; }
        public abstract bool ShowLocalAuthority { get; }

        public abstract bool ShowMiddleNames { get; }
        public abstract string DownloadSelectedLink { get; }

        public abstract string LearnerNumberLabel { get; }

        #endregion Abstract Properties

        public BaseLearnerNumberController(ILogger<BaseLearnerNumberController> logger,
            IPaginatedSearchService paginatedSearch,
            IMyPupilListService mplService,
            ISelectionManager selectionManager,
            IContentService contentService,
            IOptions<AzureAppSettings> azureAppSettings)
        {
            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));
            _contentService = contentService ??
                throw new ArgumentNullException(nameof(contentService));
            _paginatedSearch = paginatedSearch ??
                throw new ArgumentNullException(nameof(paginatedSearch));
            _selectionManager = selectionManager ??
                throw new ArgumentNullException(nameof(selectionManager));
            _mplService = mplService ??
                throw new ArgumentNullException(nameof(mplService));
            _appSettings = azureAppSettings.Value;
        }

        #region Search

        [NonAction]
        public async Task<IActionResult> Search(bool? returnToSearch)
        {
            var model = new LearnerNumberSearchViewModel();

            PopulatePageText(model);
            PopulateNavigation(model);
            PopulateSorting(model, this.HttpContext.Session.GetString(SearchSessionSortField), this.HttpContext.Session.GetString(SearchSessionSortDirection));
            ClearSortingDataFromSession();
            LearnerNumberSearchViewModel.MaximumLearnerNumbersPerSearch = _appSettings.MaximumUPNsPerSearch;

            var newsPublication = await _contentService.GetContent(DocumentType.PublicationSchedule).ConfigureAwait(false);
            model.DataReleaseTimeTable.NewsPublication = newsPublication.ConvertToViewModel();

            model.ShowMiddleNames = this.ShowMiddleNames;

            SetModelApplicationLabels(model);

            if (returnToSearch ?? false && this.HttpContext.Session.Keys.Contains(SearchSessionKey))
            {
                ModelState.Clear();
                model.LearnerNumber = this.HttpContext.Session.GetString(SearchSessionKey);
                model = await GetPupilsForSearchBuilder(model, IndexType, 0, true).ConfigureAwait(false);
                model.PageNumber = 0;
                model.PageSize = PAGESIZE;
            }

            if (!returnToSearch.HasValue)
            {
                _selectionManager.Clear();
            }

            return View(Global.SearchView, model);
        }

        [NonAction]
        public async Task<IActionResult> Search(LearnerNumberSearchViewModel model, int pageNumber, string sortField = "", string sortDirection = "", bool hasQueryItem = false, bool calledByController = false, bool resetSelections = false)
        {
            _logger.LogInformation("BaseLearnerNumberController POST method called");
            if (resetSelections)
            {
                _selectionManager.Clear();
                ClearSortingDataFromSession();
            }

            PopulatePageText(model);
            PopulateNavigation(model);
            PopulateSorting(model, sortField, sortDirection);

            LearnerNumberSearchViewModel.MaximumLearnerNumbersPerSearch = _appSettings.MaximumUPNsPerSearch;
            if (!string.IsNullOrEmpty(model.LearnerNumber))
            {
                model.LearnerNumber = Regex.Replace(model.LearnerNumber, @"[ \t]", "");
            }
            var notPaged = hasQueryItem && !calledByController;
            var allSelected = false;

            var newsPublication = await _contentService.GetContent(DocumentType.PublicationSchedule).ConfigureAwait(false);
            model.NewsPublication = newsPublication.ConvertToViewModel();

            model.ShowMiddleNames = this.ShowMiddleNames;

            model.SearchBoxErrorMessage = ModelState.IsValid is false ? GenerateValidationMessage() : null;

            model.LearnerNumber = SecurityHelper.SanitizeText(model.LearnerNumber);

            if (ModelState.IsValid)
            {
                SetModelApplicationLabels(model);

                if (!String.IsNullOrEmpty(model.SelectAllNoJsChecked))
                {
                    var selectAll = Convert.ToBoolean(model.SelectAllNoJsChecked);
                    var LearnerNumbers = model.LearnerNumberIds.FormatLearnerNumbers();
                    if (selectAll)
                    {
                        _selectionManager.AddAll(LearnerNumbers);
                        model.ToggleSelectAll = true;
                    }
                    else
                    {
                        _selectionManager.RemoveAll(LearnerNumbers);
                        model.ToggleSelectAll = false;
                    }

                    model.SelectAllNoJsChecked = null;
                    allSelected = true;
                }

                if (!notPaged && !allSelected)
                {
                    SetSelections(
                        model.PageLearnerNumbers.Split(','),
                        model.SelectedPupil);
                }

                model = await GetPupilsForSearchBuilder(
                    model,
                    IndexType,
                    pageNumber,
                    notPaged).ConfigureAwait(false);
                model.PageNumber = pageNumber;
                model.PageSize = PAGESIZE;
            }

            this.HttpContext.Session.SetString(SearchSessionKey, model.LearnerNumber);

            _logger.LogInformation("BaseLearnerNumberController POST search method invoked");

            return View(Global.SearchView, model);
        }

        private void SetModelApplicationLabels(LearnerNumberSearchViewModel model)
        {
            model.AddSelectedToMyPupilListLink = ApplicationLabel.AddSelectedToMyPupilListLink;
            model.DownloadSelectedLink = DownloadSelectedLink;
            model.DownloadSelectedASCTFLink = ApplicationLabel.DownloadSelectedAsCtfLink;
        }

        #endregion Search

        #region Invalid Numbers

        [NonAction]
        public async Task<IActionResult> InvalidUPNs(InvalidLearnerNumberSearchViewModel model)
        {
            _logger.LogInformation("National pupil database Upn Invalid UPNs POST method called");

            model.SearchAction = SearchAction;
            model.InvalidUPNsConfirmationAction = InvalidUPNsConfirmationAction;

            model.LearnerNumber = SecurityHelper.SanitizeText(model.LearnerNumber);

            model = await GetInvalidPupils(model, IndexType).ConfigureAwait(false);

            _logger.LogInformation("National pupil database Upn Invalid UPNs POST search method invoked");

            return View(Global.InvalidUPNsView, model);
        }

        [NonAction]
        public async Task<IActionResult> InvalidUPNsConfirmation(InvalidLearnerNumberSearchViewModel model)
        {
            model.SearchAction = SearchAction;
            model.InvalidUPNsConfirmationAction = InvalidUPNsConfirmationAction;

            if (!string.IsNullOrEmpty(model.SelectedInvalidUPNOption))
            {
                switch (model.SelectedInvalidUPNOption)
                {
                    case Global.InvalidUPNConfirmation_ReturnToSearch:

                        return RedirectToAction(model.SearchAction, new { returnToSearch = true });

                    case Global.InvalidUPNConfirmation_MyPupilList: return RedirectToAction(Global.MyPupilListAction, Global.MyPupilListControllerName);
                }
            }
            else
            {
                ModelState.AddModelError("NoContinueSelection", CommonErrorMessages.NoContinueSelection);
            }

            return await InvalidUPNs(model);
        }

        #endregion Invalid Numbers

        #region MPL

        [NonAction]
        public async Task<IActionResult> AddToMyPupilList(LearnerNumberSearchViewModel model)
        {
            PopulatePageText(model);
            PopulateNavigation(model);

            SetSelections(
                model.PageLearnerNumbers.Split(','),
                model.SelectedPupil);
            var selected = GetSelected(model.LearnerNumberIds.FormatLearnerNumbers());

            if (selected.Count == 0)
            {
                model.NoPupil = true;
                model.NoPupilSelected = true;
                return await ReturnToPage(model);
            }

            var learnerList = await _mplService.GetMyPupilListLearnerNumbers(User.GetUserId());

            var formattedMPLItems = new List<MyPupilListItem>();
            foreach (var item in selected)
            {
                formattedMPLItems.Add(new MyPupilListItem(item, false));
            }

            var learnerListUnion = learnerList.Union(formattedMPLItems);

            if (learnerListUnion.Count() > MyPupilListLimit)
            {
                model.ErrorDetails = CommonErrorMessages.MyPupilListLimitExceeded;
            }
            else
            {
                var invalidUPNs = selected.Where(x => !ValidationHelper.IsValidUpn(x)).ToArray();

                if (invalidUPNs.Length > 0)
                {
                    model.SelectedPupil = model.SelectedPupil.Where(x => ValidationHelper.IsValidUpn(x)).ToList();
                    learnerListUnion = learnerListUnion.Where(x => !invalidUPNs.Contains(x.PupilId));
                }

                await _mplService.UpdateMyPupilList(learnerListUnion, User.GetUserId(), AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId()));
                model.ItemAddedToMyPupilList = true;

                if (invalidUPNs.Length > 0)
                {
                    var invalidViewModel = new InvalidLearnerNumberSearchViewModel()
                    {
                        LearnerNumber = string.Join("\n", invalidUPNs)
                    };
                    return await InvalidUPNs(invalidViewModel);
                }
            }

            return await ReturnToPage(model);
        }

        #endregion MPL

        #region Private Methods

        private async Task<LearnerNumberSearchViewModel> GetPupilsForSearchBuilder(
            LearnerNumberSearchViewModel model,
            AzureSearchIndexType indexType,
            int pageNumber,
            bool first)
        {
            if (string.IsNullOrEmpty(model.LearnerNumber)) return model;

            var learnerNumberArray = model.LearnerNumber.FormatLearnerNumbers();

            string searchText = model.LearnerNumber.ToSearchText();

            if (!first)
            {
                searchText = model.LearnerIdSearchResult;
            }

            var result = await _paginatedSearch.GetPage(
                searchText,
                null,
                first ? _appSettings.MaximumUPNsPerSearch : PAGESIZE,
                pageNumber,
                indexType,
                first ? AzureSearchQueryType.Numbers : AzureSearchQueryType.Id,
                AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId()),
                model.SortField,
                model.SortDirection
                );

            model.Total = result.Count.HasValue ? result.Count.Value : result.Learners.Count;

            var idList = SetLearnerNumberIds(result);

            var combinedIdLearnerNumberArray = learnerNumberArray.Concat(idList).ToArray();

            if (first)
            {
                model.LearnerIdSearchResult = result.GetLearnerIdsAsString();

                var learnerNumberIdSet = result.GetLearnerNumberIds();
                var learnerNumberSet = result.GetLearnerNumbers();
                model.LearnerNumberIds = string.Join("\n", learnerNumberIdSet);
                var missing = combinedIdLearnerNumberArray.Except(learnerNumberIdSet).Except(learnerNumberSet);

                this.HttpContext.Session.SetString(MISSING_LEARNER_NUMBERS_KEY, JsonConvert.SerializeObject(missing));
                this.HttpContext.Session.SetString(TOTAL_SEARCH_RESULTS, model.Total.ToString());
            }
            else
            {
                model.Total = Convert.ToInt32(this.HttpContext.Session.GetString(TOTAL_SEARCH_RESULTS));
            }

            model.NotFound = JsonConvert.DeserializeObject<List<string>>(this.HttpContext.Session.GetString(MISSING_LEARNER_NUMBERS_KEY));

            var duplicateLearnerNumbers = ValidationHelper.GetDuplicates(learnerNumberArray.ToList());

            if (duplicateLearnerNumbers.Any())
            {
                foreach (var learnerNumber in duplicateLearnerNumbers)
                {
                    model.Duplicates.Add(learnerNumber);
                }
            }

            var potentialErrorLearnerNumbers = learnerNumberArray.Distinct().ToList();

            if (potentialErrorLearnerNumbers.Any())
            {
                foreach (var learnerNumber in potentialErrorLearnerNumbers)
                {
                    bool isValid = ValidateLearnerNumber(learnerNumber);

                    if (!isValid)
                    {
                        model.Invalid.Add(learnerNumber);
                    }
                }

                _logger.LogError("Some of the LearnerNumber(s) have not been found in our database");
                //result.Reverse(); // TODO: why is this here?
            }

            // ensure that the selections are set appropriately
            var selected = GetSelected(combinedIdLearnerNumberArray);
            foreach (var learner in result.Learners)
            {
                learner.Selected = selected.Contains(learner.LearnerNumberId);
            }

            model.Learners = first ? result.Learners.Take(PAGESIZE) : result.Learners;
            model.PageLearnerNumbers = String.Join(',', model.Learners.Select(l => l.LearnerNumberId));
            return model;
        }

        private async Task<InvalidLearnerNumberSearchViewModel> GetInvalidPupils(InvalidLearnerNumberSearchViewModel model, AzureSearchIndexType indexType)
        {
            if (string.IsNullOrEmpty(model.LearnerNumber)) return model;

            var searchInput = model.LearnerNumber.ToDecryptedSearchText();

            var result = await _paginatedSearch.GetPage(
              searchInput,
                null,
                 _appSettings.MaximumUPNsPerSearch,
                0,
                indexType,
                AzureSearchQueryType.Numbers,
                AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId())
                );

            model.Learners = result.Learners ?? new List<Learner>();

            var nonUPNResult = await _paginatedSearch.GetPage(
            searchInput,
            null,
             _appSettings.MaximumUPNsPerSearch,
            0,
            indexType,
            AzureSearchQueryType.Id,
            AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId())
            );

            model.Learners = model.Learners.Union(nonUPNResult.Learners);

            return model;
        }

        private List<string> SetLearnerNumberIds(PaginatedResponse result)
        {
            var idList = new List<string>();
            foreach (var learner in result.Learners)
            {
                switch (learner.LearnerNumber)
                {
                    case "0":
                        learner.LearnerNumberId = learner.Id;
                        idList.Add(learner.Id);
                        break;

                    default:
                        learner.LearnerNumberId = learner.LearnerNumber;
                        break;
                }
                ;
            }

            return idList;
        }

        #endregion Private Methods

        #region Protected Methods

        protected HashSet<string> GetSelected(string[] available)
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

        protected virtual void SetSelections(IEnumerable<string> available, IEnumerable<string> selected)
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

        protected abstract Task<IActionResult> ReturnToPage(LearnerNumberSearchViewModel model);

        protected abstract bool ValidateLearnerNumber(string learnerNumber);

        protected abstract string GenerateValidationMessage();

        protected LearnerNumberSearchViewModel PopulatePageText(LearnerNumberSearchViewModel model)
        {
            model.PageHeading = PageHeading;
            model.LearnerNumberLabel = LearnerNumberLabel;
            model.ShowLocalAuthority = ShowLocalAuthority;

            return model;
        }

        protected LearnerNumberSearchViewModel PopulateNavigation(LearnerNumberSearchViewModel model)
        {
            model.DownloadLinksPartial = DownloadLinksPartial;
            model.InvalidUPNsConfirmationAction = InvalidUPNsConfirmationAction;
            model.SearchAction = SearchAction;
            model.FullTextLearnerSearchController = FullTextLearnerSearchController;
            model.FullTextLearnerSearchAction = FullTextLearnerSearchAction;
            return model;
        }

        private LearnerNumberSearchViewModel PopulateSorting(LearnerNumberSearchViewModel model, string sortField, string sortDirection)
        {
            if (!string.IsNullOrEmpty(sortField) && !string.IsNullOrEmpty(sortDirection))
            {
                SetSortingDataIntoSession(sortField, sortDirection);
                SetSortingDataIntoModel(model, sortField, sortDirection);
            }
            else if (!string.IsNullOrEmpty(this.HttpContext.Session.GetString(SearchSessionSortField)) && !string.IsNullOrEmpty(this.HttpContext.Session.GetString(SearchSessionSortDirection)))
                SetSortingDataIntoModel(model, this.HttpContext.Session.GetString(SearchSessionSortField), this.HttpContext.Session.GetString(SearchSessionSortDirection));
            return model;
        }

        // Stores sorting data into session to make it reachable on returnToSearch
        protected void SetSortingDataIntoSession(string sortField, string sortDirection)
        {
            this.HttpContext.Session.SetString(SearchSessionSortField, sortField);
            this.HttpContext.Session.SetString(SearchSessionSortDirection, sortDirection);
        }

        protected LearnerNumberSearchViewModel SetSortingDataIntoModel(LearnerNumberSearchViewModel model, string sortField, string sortDirection)
        {
            model.SortField = sortField;
            model.SortDirection = sortDirection;
            return model;
        }

        private void ClearSortingDataFromSession()
        {
            if (this.HttpContext.Session.Keys.Contains(SearchSessionSortField) && this.HttpContext.Session.Keys.Contains(SearchSessionSortDirection))
            {
                this.HttpContext.Session.Remove(SearchSessionSortField);
                this.HttpContext.Session.Remove(SearchSessionSortDirection);
            }
        }

        #endregion Protected Methods
    }
}
