using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Service.Download;
using DfE.GIAP.Service.Download.CTF;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation;
using DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.Response;
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
    private readonly IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest> _deletePupilsFromMyPupilsuseCase;
    private readonly IMyPupilsPresentationService _myPupilsPresentationService;
    private readonly IDownloadCommonTransferFileService _ctfService;
    private readonly IDownloadService _downloadService;
    private readonly AzureAppSettings _appSettings;

    public MyPupilListController(
        ILogger<MyPupilListController> logger,
        IMyPupilsPresentationService myPupilsPresentationService,
        IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest> deletePupilsFromMyPupilsuseCase,
        IOptions<AzureAppSettings> azureAppSettings,
        IDownloadCommonTransferFileService ctfService,
        IDownloadService downloadService)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;

        ArgumentNullException.ThrowIfNull(myPupilsPresentationService);
        _myPupilsPresentationService = myPupilsPresentationService;

        ArgumentNullException.ThrowIfNull(deletePupilsFromMyPupilsuseCase);
        _deletePupilsFromMyPupilsuseCase = deletePupilsFromMyPupilsuseCase;
        
        ArgumentNullException.ThrowIfNull(azureAppSettings);
        ArgumentNullException.ThrowIfNull(azureAppSettings.Value);
        _appSettings = azureAppSettings.Value;

        ArgumentNullException.ThrowIfNull(ctfService);
        _ctfService = ctfService;

        ArgumentNullException.ThrowIfNull(downloadService);
        _downloadService = downloadService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(MyPupilsErrorModel? error = null)
    {
        _logger.LogInformation("My pupil list GET method is called");

        GetPupilsForUserFromPresentationStateResponse response = await _myPupilsPresentationService.GetPupilsForUserFromPresentationStateAsync(User.GetUserId());

        MyPupilsViewModel viewModel = new(
            pupils: response.Pupils,
            error:!string.IsNullOrEmpty(error?.Message) ? error : null)
        {
            PageNumber = response.Page,
            SortDirection = response.SortDirection,
            SortField = response.SortBy,
            IsAnyPupilsSelected = response.IsAnyPupilsSelected,
            SelectAll = response.IsAllPupilsSelected,
        };

        return View(Routes.MyPupilList.MyPupilListView, viewModel);
    }


    [HttpPost]
    public async Task<IActionResult> MyPupilList(MyPupilsFormStateRequestDto formDto)
    {
        _logger.LogInformation("My pupil list POST method called");

        if (formDto.Error is not null)
        {
            return await Index(formDto.Error);
        }

        if (!ModelState.IsValid)
        {
            MyPupilsErrorModel error = new(PupilHelper.GenerateValidationMessageUpnSearch(ModelState));
            return await Index(error);
        }

        _myPupilsPresentationService.UpdatePresentationState(formDto);
        
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Route(Routes.MyPupilList.RemoveSelected)]
    public async Task<IActionResult> RemoveSelected(MyPupilsFormStateRequestDto formDto)
    {
        _logger.LogInformation("Remove from my pupil list POST method is called");

        if (!ModelState.IsValid)
        {
            MyPupilsErrorModel error = new(PupilHelper.GenerateValidationMessageUpnSearch(ModelState));
            return await Index(error);
        }

        if (formDto.SelectedPupils.Count == 0 || !formDto.SelectAll.HasValue)
        {
            MyPupilsErrorModel myPupilsErrorModel = new(Messages.Common.Errors.NoPupilsSelected);
            return await Index(myPupilsErrorModel);
        }

        DeletePupilsFromMyPupilsRequest deletePupilsRequest = new(
            UserId: User.GetUserId(),
            DeletePupilUpns: formDto.SelectedPupils,
            DeleteAll: formDto.SelectAll ?? false);

        await _deletePupilsFromMyPupilsuseCase.HandleRequestAsync(deletePupilsRequest);
        _myPupilsPresentationService.ClearPresentationState();
        return RedirectToAction(nameof(Index));
    }


    // Downloads

    [HttpPost]
    [Route(Routes.DownloadCommonTransferFile.DownloadCommonTransferFileAction)]
    public Task<IActionResult> ToDownloadCommonTransferFileData(MyPupilsFormStateRequestDto formDto) => HandleDownloadRequest(DownloadType.CTF, formDto);

    [HttpPost]
    [Route(Routes.PupilPremium.LearnerNumberDownloadRequest)]
    public Task<IActionResult> ToDownloadSelectedPupilPremiumDataUPN(MyPupilsFormStateRequestDto formDto) => HandleDownloadRequest(DownloadType.PupilPremium, formDto);

    [HttpPost]
    [Route(Routes.NationalPupilDatabase.LearnerNumberDownloadRequest)]
    public Task<IActionResult> ToDownloadSelectedNPDDataUPN(MyPupilsFormStateRequestDto formDto) => HandleDownloadRequest(DownloadType.NPD, formDto);

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
    [Route(Routes.NationalPupilDatabase.LearnerNumberDownloadFile)]
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
        MyPupilsFormStateRequestDto formDto)
    {

        IEnumerable<string> selectedPupils =  await _myPupilsPresentationService.GetSelectedPupilsForUserAsync(User.GetUserId());

        if (!selectedPupils.Any())
        {
            MyPupilsErrorModel error = new(Messages.Common.Errors.NoPupilsSelected);
            return await Index(error);
        }

        if (downloadType == DownloadType.CTF && selectedPupils.Count() > _appSettings.CommonTransferFileUPNLimit)
        {
            MyPupilsErrorModel error = new(Messages.Downloads.Errors.UPNLimitExceeded);
            return View(Routes.MyPupilList.MyPupilListView, error);
        }

        return downloadType switch
        {
            DownloadType.CTF => await DownloadCommonTransferFileData(formDto, selectedPupils.ToArray()),
            DownloadType.PupilPremium => await DownloadPupilPremiumData(formDto, selectedPupils.ToArray()),
            DownloadType.NPD => await DownloadSelectedNationalPupilDatabaseData(string.Join(',', selectedPupils), selectedPupils.Count()),
            _ => await Index(new MyPupilsErrorModel(Messages.Downloads.Errors.UnknownDownloadType))
        };
    }

    private async Task<IActionResult> DownloadCommonTransferFileData(MyPupilsFormStateRequestDto formRequestDto, string[] selectedPupils)
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

        MyPupilsFormStateRequestDto errorFormState = new()
        {
            SelectAll = formRequestDto.SelectAll,
            SelectedPupils = formRequestDto.SelectedPupils,
            CurrentPageOfPupils = formRequestDto.CurrentPageOfPupils,
            PageNumber = formRequestDto.PageNumber,
            SortDirection = formRequestDto.SortDirection,
            SortField = formRequestDto.SortField,
            Error = new MyPupilsErrorModel(Messages.Downloads.Errors.NoDataForSelectedPupils)
        }; 

        return await MyPupilList(errorFormState);
    }

    private async Task<IActionResult> DownloadPupilPremiumData(MyPupilsFormStateRequestDto formRequestDto, string[] selectedPupils)
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
            return RedirectToAction(actionName: Routes.Application.Error, controllerName: Routes.Application.Home);
        }

        if (downloadFile.Bytes != null)
        {
            return SearchDownloadHelper.DownloadFile(downloadFile);
        }

        MyPupilsFormStateRequestDto errorFormState = new()
        {
            SelectAll = formRequestDto.SelectAll,
            SelectedPupils = formRequestDto.SelectedPupils,
            CurrentPageOfPupils = formRequestDto.CurrentPageOfPupils,
            PageNumber = formRequestDto.PageNumber,
            SortDirection = formRequestDto.SortDirection,
            SortField = formRequestDto.SortField,
            Error = new MyPupilsErrorModel(Messages.Downloads.Errors.NoDataForSelectedPupils)
        };

        return await MyPupilList(errorFormState);
    }
}
