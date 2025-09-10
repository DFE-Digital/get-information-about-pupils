using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Core.MyPupils.Application.Extensions;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.Users.Application;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Service.Download;
using DfE.GIAP.Service.Download.CTF;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Features.MyPupils.Services.GetSelectedMyPupils;
using DfE.GIAP.Web.Features.MyPupils.ViewModel;
using DfE.GIAP.Web.Helpers.SearchDownload;
using DfE.GIAP.Web.ViewModels.Search;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Web.Features.MyPupils.Routes;

[Route(Constants.Routes.Application.MyPupilList)]
public class MyPupilsDownloadController : Controller
{
    private readonly ILogger<MyPupilsDownloadController> _logger;
    private readonly AzureAppSettings _appSettings;
    private readonly IDownloadCommonTransferFileService _ctfService;
    private readonly IDownloadService _downloadService;
    private readonly IGetSelectedMyPupilsProvider _getSelectedMyPupilsProvider;
    private readonly IMyPupilsViewModelFactory _myPupilsViewModelFactory;

    public MyPupilsDownloadController(
        ILogger<MyPupilsDownloadController> logger,
        IOptions<AzureAppSettings> azureAppSettings,
        IDownloadCommonTransferFileService ctfService,
        IDownloadService downloadService,
        IGetSelectedMyPupilsProvider getSelectedMyPupilsProvider,
        IMyPupilsViewModelFactory myPupilsViewModelFactory)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;

        ArgumentNullException.ThrowIfNull(azureAppSettings);
        ArgumentNullException.ThrowIfNull(azureAppSettings.Value);
        _appSettings = azureAppSettings.Value;

        ArgumentNullException.ThrowIfNull(ctfService);
        _ctfService = ctfService;

        ArgumentNullException.ThrowIfNull(downloadService);
        _downloadService = downloadService;

        ArgumentNullException.ThrowIfNull(getSelectedMyPupilsProvider);
        _getSelectedMyPupilsProvider = getSelectedMyPupilsProvider;

        ArgumentNullException.ThrowIfNull(myPupilsViewModelFactory);
        _myPupilsViewModelFactory = myPupilsViewModelFactory;
    }

    [HttpPost]
    [Route(Constants.Routes.DownloadCommonTransferFile.DownloadCommonTransferFileAction)]
    public Task<IActionResult> ToDownloadCommonTransferFileData([FromForm] List<string> SelectedPupils) => HandleDownloadRequest(DownloadType.CTF, SelectedPupils);

    [HttpPost]
    [Route(Constants.Routes.PupilPremium.LearnerNumberDownloadRequest)]
    public Task<IActionResult> ToDownloadSelectedPupilPremiumDataUPN([FromForm] List<string> SelectedPupils) => HandleDownloadRequest(DownloadType.PupilPremium, SelectedPupils);

    [HttpPost]
    [Route(Constants.Routes.NationalPupilDatabase.LearnerNumberDownloadRequest)]
    public Task<IActionResult> ToDownloadSelectedNPDDataUPN([FromForm] List<string> SelectedPupils) => HandleDownloadRequest(DownloadType.NPD, SelectedPupils);

    [HttpPost]
    [Route(Constants.Routes.DownloadSelectedNationalPupilDatabaseData)]
    public async Task<IActionResult> DownloadSelectedNationalPupilDatabaseData(string selectedPupilsJoined)
    {
        LearnerDownloadViewModel searchDownloadViewModel = new()
        {
            SelectedPupils = selectedPupilsJoined,
            ErrorDetails = (string)TempData["ErrorDetails"],
            SelectedPupilsCount = selectedPupilsJoined.Length,
            DownloadFileType = DownloadFileType.CSV,
            ShowTABDownloadType = true
        };

        SearchDownloadHelper.AddDownloadDataTypes(
            searchDownloadViewModel,
            User,
            User.GetOrganisationLowAge(),
            User.GetOrganisationHighAge(),
            User.IsOrganisationLocalAuthority(),
            User.IsOrganisationAllAges());

        LearnerNumberSearchViewModel.MaximumLearnerNumbersPerSearch = _appSettings.MaximumUPNsPerSearch;
        ModelState.Clear();
        searchDownloadViewModel.NumberSearchViewModel.LearnerNumber = selectedPupilsJoined.Replace(",", "\r\n");
        searchDownloadViewModel.SearchAction = "MyPupilList";
        searchDownloadViewModel.DownloadRoute = Constants.Routes.NationalPupilDatabase.LearnerNumberDownloadFile;

        string[] selectedPupils = selectedPupilsJoined.Split(',');
        if (selectedPupils.Length < _appSettings.DownloadOptionsCheckLimit)
        {
            string[] downloadTypeArray = searchDownloadViewModel.SearchDownloadDatatypes.Select(d => d.Value).ToArray();
            IEnumerable<CheckDownloadDataType> disabledTypes = await _downloadService.CheckForNoDataAvailable(
                selectedPupils,
                selectedPupils,
                downloadTypeArray,
                AzureFunctionHeaderDetails.Create(
                    User.GetUserId(),
                    User.GetSessionId()));

            SearchDownloadHelper.DisableDownloadDataTypes(searchDownloadViewModel, disabledTypes);
        }

        searchDownloadViewModel.SearchResultPageHeading = ApplicationLabels.SearchMyPupilListPageHeading;

        return View(Global.MPLDownloadNPDOptionsView, searchDownloadViewModel);
    }

    [HttpPost]
    [Route(Constants.Routes.NationalPupilDatabase.LearnerNumberDownloadFile)]
    public async Task<IActionResult> DownloadSelectedNationalPupilDatabaseData(LearnerDownloadViewModel model)
    {
        if (!string.IsNullOrEmpty(model.SelectedPupils))
        {
            string[] selectedPupils = model.SelectedPupils.Split(',');

            if (model.SelectedDownloadOptions == null)
            {
                model.ErrorDetails = Messages.Search.Errors.SelectOneOrMoreDataTypes;
            }
            else if (model.DownloadFileType != DownloadFileType.None)
            {
                ReturnFile downloadFile = model.DownloadFileType == DownloadFileType.CSV
                    ? await _downloadService.GetCSVFile(selectedPupils, selectedPupils, model.SelectedDownloadOptions, true, AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId()), ReturnRoute.NationalPupilDatabase)
                    : await _downloadService.GetTABFile(selectedPupils, selectedPupils, model.SelectedDownloadOptions, true, AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId()), ReturnRoute.NationalPupilDatabase);

                if (downloadFile == null)
                {
                    return base.RedirectToAction(Constants.Routes.Application.Error, Constants.Routes.Application.Home);
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
            return await DownloadSelectedNationalPupilDatabaseData(model.SelectedPupils);
        }

        return RedirectToAction(Global.MyPupilListAction, Global.MyPupilListControllerName);
    }


    private async Task<IActionResult> HandleDownloadRequest(
        DownloadType downloadType,
        List<string> selectedPupils)
    {
        List<UniquePupilNumber> selectedPupilsFromForm = selectedPupils?.ToUniquePupilNumbers().ToList() ?? [];
        UniquePupilNumbers selectedPupilsFromState = _getSelectedMyPupilsProvider.GetSelectedMyPupils();

        UniquePupilNumbers joinedSelectedPupils =
            UniquePupilNumbers.Create(
                uniquePupilNumbers:
                    selectedPupilsFromForm.Concat(selectedPupilsFromState.GetUniquePupilNumbers()).Distinct());

        UserId user = new(User.GetUserId());

        if (joinedSelectedPupils.IsEmpty)
        {
            MyPupilsErrorViewModel error = new(Messages.Common.Errors.NoPupilsSelected);
            MyPupilsViewModel viewModel = await _myPupilsViewModelFactory.CreateViewModelAsync(user, error);
            return base.View(Constants.Routes.MyPupilList.MyPupilListView, viewModel);
        }

        // TODO do I need to UpdateSelectedPupilsHere in state, as the user may have selected pupils on page, and then want to download other data?
        // Maybe a service called MergedSelectionStateService which calls PaginatedMyPupilsHandler, creates this object, that can then be passed down instead of MyPupilsFormStateDto
        // Dictionary<UniquePupilNumber, bool> --- then do the CurrentPage

        return downloadType switch
        {
            DownloadType.CTF => await DownloadCommonTransferFileData(joinedSelectedPupils),
            DownloadType.PupilPremium => await DownloadPupilPremiumData(joinedSelectedPupils),
            DownloadType.NPD => await DownloadSelectedNationalPupilDatabaseData(string.Join(",", joinedSelectedPupils.GetUniquePupilNumbers().Select(t => t.Value))),
            _ => base.View(
                Constants.Routes.MyPupilList.MyPupilListView,
                await _myPupilsViewModelFactory.CreateViewModelAsync(user, new MyPupilsErrorViewModel(Messages.Downloads.Errors.UnknownDownloadType)))
        };
    }

    private async Task<IActionResult> DownloadCommonTransferFileData(UniquePupilNumbers selectedPupils)
    {
        string[] selectedPupilsInput = selectedPupils.GetUniquePupilNumbers().Select(t => t.Value).ToArray();

        UserId userId = new(User.GetUserId());

        if (selectedPupils.Count > _appSettings.CommonTransferFileUPNLimit) // TODO check this works pulled from HandleDownloadReq
        {
            MyPupilsErrorViewModel error = new(Messages.Downloads.Errors.UPNLimitExceeded);
            await _myPupilsViewModelFactory.CreateViewModelAsync(userId, error);

            return View(Constants.Routes.MyPupilList.MyPupilListView, error);
        }

        ReturnFile downloadFile = await _ctfService.GetCommonTransferFile(
            selectedPupilsInput,
            selectedPupilsInput,
            User.GetLocalAuthorityNumberForEstablishment(),
            User.GetEstablishmentNumber(),
            User.IsOrganisationEstablishment(),
            AzureFunctionHeaderDetails.Create(
                userId.Value,
                User.GetSessionId()),
            ReturnRoute.MyPupilList);

        if (downloadFile.Bytes != null)
        {
            return SearchDownloadHelper.DownloadFile(downloadFile);
        }

        MyPupilsErrorViewModel noDataAvailableError = new(Messages.Downloads.Errors.NoDataForSelectedPupils);
        MyPupilsViewModel viewModel = await _myPupilsViewModelFactory.CreateViewModelAsync(userId, error: noDataAvailableError);

        return base.View(Constants.Routes.MyPupilList.MyPupilListView, viewModel);
    }

    private async Task<IActionResult> DownloadPupilPremiumData(UniquePupilNumbers selectedPupils)
    {
        UserOrganisation userOrganisation = new()
        {
            IsAdmin = User.IsAdmin(),
            IsEstablishment = User.IsOrganisationEstablishment(),
            IsLa = User.IsOrganisationLocalAuthority(),
            IsMAT = User.IsOrganisationMultiAcademyTrust(),
            IsSAT = User.IsOrganisationSingleAcademyTrust()
        };

        UserId userId = new(User.GetUserId());

        string[] selectedPupilInput = selectedPupils.GetUniquePupilNumbers().Select(t => t.Value).ToArray();

        ReturnFile downloadFile = await _downloadService.GetPupilPremiumCSVFile(
            selectedPupilInput,
            selectedPupilInput,
            true,
            AzureFunctionHeaderDetails.Create(userId.Value, User.GetSessionId()),
            ReturnRoute.MyPupilList, userOrganisation);

        if (downloadFile == null)
        {
            return RedirectToAction(actionName: Constants.Routes.Application.Error, controllerName: Constants.Routes.Application.Home);
        }

        if (downloadFile.Bytes != null)
        {
            return SearchDownloadHelper.DownloadFile(downloadFile);
        }
        
        MyPupilsErrorViewModel noDataAvailableError = new(Messages.Downloads.Errors.NoDataForSelectedPupils);
        MyPupilsViewModel viewModel = await _myPupilsViewModelFactory.CreateViewModelAsync(userId, noDataAvailableError);
        return base.View(Constants.Routes.MyPupilList.MyPupilListView, viewModel);
    }
}
