using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Service.Download;
using DfE.GIAP.Service.Download.CTF;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Features.MyPupils.Services.GetSelectedMyPupils;
using DfE.GIAP.Web.Features.MyPupils.State;
using DfE.GIAP.Web.Features.MyPupils.State.Selection;
using DfE.GIAP.Web.Features.MyPupils.ViewModel;
using DfE.GIAP.Web.Helpers.SearchDownload;
using DfE.GIAP.Web.Session.Abstraction.Command;
using DfE.GIAP.Web.ViewModels.Search;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DfE.GIAP.Web.Features.MyPupils.Routes;

[Route(Constants.Routes.MyPupilList.MyPupils)]
public class DownloadMyPupilsController : Controller
{
    private readonly ILogger<DownloadMyPupilsController> _logger;
    private readonly AzureAppSettings _appSettings;
    private readonly IDownloadCommonTransferFileService _ctfService;
    private readonly IDownloadService _downloadService;
    private readonly IGetSelectedMyPupilsHandler _getSelectedMyPupilsProvider;
    private readonly IGetMyPupilsStateProvider _getMyPupilsStateProvider;
    private readonly IMyPupilsViewModelFactory _myPupilsViewModelFactory;
    private readonly ISessionCommandHandler<MyPupilsPupilSelectionState> _selectionStateSessionCommandHandler;

    public DownloadMyPupilsController(
        ILogger<DownloadMyPupilsController> logger,
        IOptions<AzureAppSettings> azureAppSettings,
        IDownloadCommonTransferFileService ctfService,
        IDownloadService downloadService,
        IGetSelectedMyPupilsHandler getSelectedMyPupilsProvider,
        IMyPupilsViewModelFactory myPupilsViewModelFactory,
        ISessionCommandHandler<MyPupilsPupilSelectionState> selectionStateSessionCommandHandler,
        IGetMyPupilsStateProvider getMyPupilsStateProvider)
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

        ArgumentNullException.ThrowIfNull(selectionStateSessionCommandHandler);
        _selectionStateSessionCommandHandler = selectionStateSessionCommandHandler;
        
        ArgumentNullException.ThrowIfNull(getMyPupilsStateProvider);
        _getMyPupilsStateProvider = getMyPupilsStateProvider;
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
        string[] selectedPupils = selectedPupilsJoined.Split(',');

        LearnerDownloadViewModel searchDownloadViewModel = new()
        {
            SelectedPupils = selectedPupilsJoined,
            ErrorDetails = (string)TempData["ErrorDetails"],
            SelectedPupilsCount = selectedPupils.Length,
            DownloadFileType = DownloadFileType.CSV,
            ShowTABDownloadType = true,
            DownloadRoute = Constants.Routes.NationalPupilDatabase.LearnerNumberDownloadFile,
            SearchResultPageHeading = ApplicationLabels.SearchMyPupilListPageHeading
        };

        LearnerNumberSearchViewModel.MaximumLearnerNumbersPerSearch = _appSettings.MaximumUPNsPerSearch;
        searchDownloadViewModel.NumberSearchViewModel.LearnerNumber = selectedPupilsJoined.Replace(",", "\r\n");

        SearchDownloadHelper.AddDownloadDataTypes(
            searchDownloadViewModel,
            User,
            User.GetOrganisationLowAge(),
            User.GetOrganisationHighAge(),
            User.IsOrganisationLocalAuthority(),
            User.IsOrganisationAllAges());

        ModelState.Clear();

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
        List<string> formSelectedPupils)
    {
        string userId = User.GetUserId();

        if (formSelectedPupils.Count > 0)
        {
            MyPupilsPupilSelectionState selectionState = _getMyPupilsStateProvider.GetState().SelectionState;
            selectionState.UpsertPupilSelectionState(formSelectedPupils, true);
            _selectionStateSessionCommandHandler.StoreInSession(selectionState);
        }

        string[] allSelectedPupils =
            _getSelectedMyPupilsProvider.GetSelectedMyPupils()
                .Concat(formSelectedPupils)
                .Distinct()
                .ToArray();

        if (allSelectedPupils.Length == 0)
        {
            MyPupilsErrorViewModel error = MyPupilsErrorViewModel.Create(Messages.Common.Errors.NoPupilsSelected);
            MyPupilsViewModel viewModel = await _myPupilsViewModelFactory.CreateViewModelAsync(userId, new MyPupilsViewModelContext(error));

            return View(Constants.Routes.MyPupilList.MyPupilListView, viewModel);
        }

        if (downloadType == DownloadType.CTF && allSelectedPupils.Length > _appSettings.CommonTransferFileUPNLimit)
        {
            MyPupilsErrorViewModel error = MyPupilsErrorViewModel.Create(Messages.Downloads.Errors.UPNLimitExceeded);
            MyPupilsViewModel viewModel = await _myPupilsViewModelFactory.CreateViewModelAsync(userId, new MyPupilsViewModelContext(error));

            return View(Constants.Routes.MyPupilList.MyPupilListView, viewModel);
        }

        if (downloadType == DownloadType.CTF)
        {
            ReturnFile downloadFile = await _ctfService.GetCommonTransferFile(
                allSelectedPupils,
                allSelectedPupils,
                User.GetLocalAuthorityNumberForEstablishment(),
                User.GetEstablishmentNumber(),
                User.IsOrganisationEstablishment(),
                AzureFunctionHeaderDetails.Create(
                    userId,
                    User.GetSessionId()),
                ReturnRoute.MyPupilList);

            if (downloadFile.Bytes != null)
            {
                return SearchDownloadHelper.DownloadFile(downloadFile);
            }

            MyPupilsErrorViewModel noDataAvailableError = MyPupilsErrorViewModel.Create(Messages.Downloads.Errors.NoDataForSelectedPupils);
            MyPupilsViewModel viewModel = await _myPupilsViewModelFactory.CreateViewModelAsync(userId, new MyPupilsViewModelContext(noDataAvailableError));

            return View(Constants.Routes.MyPupilList.MyPupilListView, viewModel);
        }

        if(downloadType == DownloadType.PupilPremium)
        {
            UserOrganisation userOrganisation = new()
            {
                IsAdmin = User.IsAdmin(),
                IsEstablishment = User.IsOrganisationEstablishment(),
                IsLa = User.IsOrganisationLocalAuthority(),
                IsMAT = User.IsOrganisationMultiAcademyTrust(),
                IsSAT = User.IsOrganisationSingleAcademyTrust()
            };


            ReturnFile downloadFile = await _downloadService.GetPupilPremiumCSVFile(
                allSelectedPupils,
                allSelectedPupils,
                true,
                AzureFunctionHeaderDetails.Create(
                    userId,
                    User.GetSessionId()),
                ReturnRoute.MyPupilList,
                userOrganisation);

            if (downloadFile == null)
            {
                return RedirectToAction(actionName: Constants.Routes.Application.Error, controllerName: Constants.Routes.Application.Home);
            }

            if (downloadFile.Bytes != null)
            {
                return SearchDownloadHelper.DownloadFile(downloadFile);
            }

            MyPupilsErrorViewModel noDataAvailableError = MyPupilsErrorViewModel.Create(Messages.Downloads.Errors.NoDataForSelectedPupils); 
            MyPupilsViewModel viewModel = await _myPupilsViewModelFactory.CreateViewModelAsync(userId, new MyPupilsViewModelContext(noDataAvailableError));
            return View(Constants.Routes.MyPupilList.MyPupilListView, viewModel);
        }

        if(downloadType == DownloadType.NPD)
        {
            return await DownloadSelectedNationalPupilDatabaseData(string.Join(",", allSelectedPupils));
        }

        MyPupilsErrorViewModel unknownDownloadTypeError = MyPupilsErrorViewModel.Create(Messages.Downloads.Errors.UnknownDownloadType);
        MyPupilsViewModelContext context = new(unknownDownloadTypeError);
        MyPupilsViewModel errorViewModel = await _myPupilsViewModelFactory.CreateViewModelAsync(userId, context);
        return View(Constants.Routes.MyPupilList.MyPupilListView, errorViewModel);
    }
}
