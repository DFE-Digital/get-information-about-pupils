using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Request;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Service.Download;
using DfE.GIAP.Service.Download.CTF;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Controllers.MyPupilList.FormState;
using DfE.GIAP.Web.Controllers.MyPupilList.Services.Presenter;
using DfE.GIAP.Web.Controllers.MyPupilList.ViewModel;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Helpers.Search;
using DfE.GIAP.Web.Helpers.SearchDownload;
using DfE.GIAP.Web.ViewModels.Search;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Web.Controllers.MyPupilList;

[Route(Routes.Application.MyPupilList)]
public class MyPupilListController : Controller
{
    private readonly ILogger<MyPupilListController> _logger;
    private readonly IDownloadCommonTransferFileService _ctfService;
    private readonly IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest> _deletePupilsFromMyPupilsuseCase;
    private readonly IDownloadService _downloadService;
    private readonly AzureAppSettings _appSettings;
    private readonly IMyPupilsPresentationService _myPupilsPresentationService;

    public MyPupilListController(
        ILogger<MyPupilListController> logger,
        IDownloadCommonTransferFileService ctfService,
        IDownloadService downloadService,
        IOptions<AzureAppSettings> azureAppSettings,
        IUseCase<GetMyPupilsRequest, GetMyPupilsResponse> getMyPupilsUseCase,
        IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest> deletePupilsFromMyPupilsuseCase,
        IMyPupilsPresentationService myPupilsPresentationService)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;

        ArgumentNullException.ThrowIfNull(deletePupilsFromMyPupilsuseCase);
        _deletePupilsFromMyPupilsuseCase = deletePupilsFromMyPupilsuseCase;

        ArgumentNullException.ThrowIfNull(ctfService);
        _ctfService = ctfService;

        ArgumentNullException.ThrowIfNull(azureAppSettings);
        ArgumentNullException.ThrowIfNull(azureAppSettings.Value);
        _appSettings = azureAppSettings.Value;

        ArgumentNullException.ThrowIfNull(downloadService);
        _downloadService = downloadService;

        ArgumentNullException.ThrowIfNull(myPupilsPresentationService);
        _myPupilsPresentationService = myPupilsPresentationService;
    }

    // TODO move GetMyPupils behind the PresentationService, and just give it my UserId.

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        _logger.LogInformation("My pupil list GET method is called");

        MyPupilsFormState myPupilsFormState = new();

        IEnumerable<PupilPresentatationModel> pupils = await _myPupilsPresentationService.GetPupilsForUserAsync(
            User.GetUserId(),
            myPupilsFormState);

        MyPupilsViewModel viewModel = new(pupils, myPupilsFormState);

        return View(Routes.MyPupilList.MyPupilListView, viewModel);
    }

    // TODO introduce GET so that back works?

    [HttpPost]
    public async Task<IActionResult> MyPupilList(
        [FromForm] MyPupilsFormRequestDto formData,
        [FromQuery] int pageNumber = 1,
        [FromQuery] string sortField = "",
        [FromQuery] string sortDirection = "")
    {

        _logger.LogInformation("My pupil list UPN POST method called");

        MyPupilsFormState formState = new(
            pageNumber,
            sortField,
            sortDirection,
            formData.SelectedPupils,
            formData.SelectAll);

        if (!ModelState.IsValid)
        {
            MyPupilsErrorModel error = new(PupilHelper.GenerateValidationMessageUpnSearch(ModelState));
            MyPupilsViewModel outputModelOnError = new([], formState, error);
            return View(Routes.MyPupilList.MyPupilListView, outputModelOnError);
        }

        IEnumerable<PupilPresentatationModel> pupils = await _myPupilsPresentationService.GetPupilsForUserAsync(
            User.GetUserId(),
            formState);

        MyPupilsViewModel viewModel = new(pupils, formState);

        if (formData.Error is not null)
        {
            viewModel.SetError(formData.Error);
        }

        return View(Routes.MyPupilList.MyPupilListView, viewModel);
    }

    [HttpPost]
    [Route(Routes.MyPupilList.RemoveSelected)]
    public async Task<IActionResult> RemoveSelected(
        [FromForm] MyPupilsFormRequestDto formData,
        [FromQuery] int pageNumber,
        [FromQuery] string sortField,
        [FromQuery] string sortDirection)
    {
        _logger.LogInformation("Remove from my pupil list POST method is called");

        MyPupilsFormState formState = new(
            pageNumber,
            sortField,
            sortDirection,
            formData.SelectedPupils,
            formData.SelectAll);

        if (formState.NoSelectionsForPupilsMade)
        {
            IEnumerable<PupilPresentatationModel> pupils = await _myPupilsPresentationService.GetPupilsForUserAsync(User.GetUserId(), formState);

            MyPupilsViewModel outputModelOnError = new(
                pupils,
                formState,
                new MyPupilsErrorModel(Messages.Common.Errors.NoPupilsSelected));

            return View(Routes.MyPupilList.MyPupilListView, outputModelOnError);
        }

        if (!ModelState.IsValid)
        {
            // TODO error handle
        }

        DeletePupilsFromMyPupilsRequest deletePupilsRequest = new(
            UserId: User.GetUserId(),
            DeletePupilUpns: formData.SelectedPupils,
            DeleteAll: formData.SelectAll);

        await _deletePupilsFromMyPupilsuseCase.HandleRequestAsync(deletePupilsRequest);

        TempData["IsRemoveSuccessful"] = true;

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Route(Routes.DownloadCommonTransferFile.DownloadCommonTransferFileAction)]
    public Task<IActionResult> ToDownloadCommonTransferFileData(
        [FromForm] MyPupilsFormRequestDto formData,
        [FromQuery] int pageNumber,
        [FromQuery] string sortField,
        [FromQuery] string sortDirection)
    {
        MyPupilsFormState formState = new(
            pageNumber,
            sortField,
            sortDirection,
            formData.SelectedPupils,
            formData.SelectAll);

        return HandleDownloadRequest(DownloadType.CTF, formState);
    }

    [HttpPost]
    [Route(Routes.PupilPremium.LearnerNumberDownloadRequest)]
    public Task<IActionResult> ToDownloadSelectedPupilPremiumDataUPN(
        [FromForm] MyPupilsFormRequestDto formData,
        [FromQuery] int pageNumber,
        [FromQuery] string sortField,
        [FromQuery] string sortDirection)
    {
        MyPupilsFormState formState = new(
            pageNumber,
            sortField,
            sortDirection,
            formData.SelectedPupils,
            formData.SelectAll);

        return HandleDownloadRequest(DownloadType.PupilPremium, formState);
    }

    [HttpPost]
    [Route(Routes.NationalPupilDatabase.LearnerNumberDownloadRequest)]
    public Task<IActionResult> ToDownloadSelectedNPDDataUPN(
        [FromForm] MyPupilsFormRequestDto formData,
        [FromQuery] int pageNumber,
        [FromQuery] string sortField,
        [FromQuery] string sortDirection)
    {
        MyPupilsFormState formState = new(
            pageNumber,
            sortField,
            sortDirection,
            formData.SelectedPupils,
            formData.SelectAll);

        return HandleDownloadRequest(DownloadType.NPD, formState);
    }

    [HttpPost]
    [Route(Routes.DownloadSelectedNationalPupilDatabaseData)]
    public async Task<IActionResult> DownloadSelectedNationalPupilDatabaseData(
        string selectedPupilsJoined,
        int selectedPupilsCount)
    {
        LearnerDownloadViewModel searchDownloadViewModel = new()
        {
            SelectedPupils = selectedPupilsJoined,
            ErrorDetails = (string)TempData["ErrorDetails"],
            SelectedPupilsCount = selectedPupilsCount,
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
        searchDownloadViewModel.DownloadRoute = Routes.NationalPupilDatabase.LearnerNumberDownloadFile;

        var selectedPupils = selectedPupilsJoined.Split(',');
        if (selectedPupils.Length < _appSettings.DownloadOptionsCheckLimit)
        {
            var downloadTypeArray = searchDownloadViewModel.SearchDownloadDatatypes.Select(d => d.Value).ToArray();
            var disabledTypes = await _downloadService.CheckForNoDataAvailable(
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
            return await DownloadSelectedNationalPupilDatabaseData(model.SelectedPupils, model.SelectedPupilsCount);
        }

        return RedirectToAction(Global.MyPupilListAction, Global.MyPupilListControllerName);
    }


    private async Task<IActionResult> HandleDownloadRequest(
        DownloadType downloadType,
        MyPupilsFormState formState)
    {
        IEnumerable<string> selectedPupils = formState.SelectedPupils.Select(
            (selectedPupil) =>
                selectedPupil
                    .Replace("\r", string.Empty)
                    .ReplaceLineEndings()
                    .Trim())
                    .ToList(); // TODO handle this in ModelBinding?

        IEnumerable<PupilPresentatationModel> pupils = await _myPupilsPresentationService.GetPupilsForUserAsync(User.GetUserId(), formState);

        if (!selectedPupils.Any())
        {
            MyPupilsViewModel outputModelOnError = new(pupils, formState, new MyPupilsErrorModel(Messages.Common.Errors.NoPupilsSelected));
            return View(Routes.MyPupilList.MyPupilListView, outputModelOnError);
        }

        if (downloadType == DownloadType.CTF && selectedPupils.Count() > _appSettings.CommonTransferFileUPNLimit)
        {
            MyPupilsViewModel outputModelOnError = new(pupils, formState, new MyPupilsErrorModel(Messages.Downloads.Errors.UPNLimitExceeded));
            return View(Routes.MyPupilList.MyPupilListView, outputModelOnError);
        }

        return downloadType switch
        {
            DownloadType.CTF => await DownloadCommonTransferFileData(formState, selectedPupils.ToArray()),
            DownloadType.PupilPremium => await DownloadPupilPremiumData(formState, selectedPupils.ToArray()),
            DownloadType.NPD => await DownloadSelectedNationalPupilDatabaseData(string.Join(',', selectedPupils), selectedPupils.Count()),
            _ => View(
                Routes.MyPupilList.MyPupilListView,
                new MyPupilsViewModel(
                    pupils,
                    formState,
                    new MyPupilsErrorModel(Messages.Downloads.Errors.UnknownDownloadType)))
        };
    }

    private async Task<IActionResult> DownloadCommonTransferFileData(MyPupilsFormState formState, string[] selectedPupils)
    {
        ReturnFile downloadFile = await _ctfService.GetCommonTransferFile(
            selectedPupils,
            User.GetLocalAuthorityNumberForEstablishment(),
            User.GetEstablishmentNumber(),
            User.IsOrganisationEstablishment(),
            AzureFunctionHeaderDetails.Create(
                User.GetUserId(),
                User.GetSessionId()),
            ReturnRoute.MyPupilList);

        if (downloadFile.Bytes != null)
        {
            return SearchDownloadHelper.DownloadFile(downloadFile);
        }

        MyPupilsFormRequestDto postBackFormState = new()
        {
            SelectAll = formState.SelectAll,
            SelectedPupils = formState.SelectedPupils.ToList(),
            Error = new MyPupilsErrorModel(Messages.Downloads.Errors.NoDataForSelectedPupils)
        };

        return await MyPupilList(
            postBackFormState,
            formState.Page.Value,
            formState.SortBy,
            formState.SortDirection.ToFormState());
    }

    private async Task<IActionResult> DownloadPupilPremiumData(MyPupilsFormState formState, string[] selectedPupils)
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
            selectedPupils, selectedPupils, true,
            AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId()),
            ReturnRoute.MyPupilList, userOrganisation);

        if (downloadFile == null)
        {
            return RedirectToAction(Routes.Application.Error, Routes.Application.Home);
        }

        if (downloadFile.Bytes != null)
        {
            return SearchDownloadHelper.DownloadFile(downloadFile);
        }

        MyPupilsFormRequestDto postBackFormState = new()
        {
            SelectAll = formState.SelectAll,
            SelectedPupils = formState.SelectedPupils.ToList(),
            Error = new MyPupilsErrorModel(Messages.Downloads.Errors.NoDataForSelectedPupils)
        };

        return await MyPupilList(
            postBackFormState,
            formState.Page.Value,
            formState.SortBy,
            formState.SortDirection.ToFormState());
    }
}
