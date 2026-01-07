using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.MyPupils.Application.Options;
using DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Features.MyPupils.Messaging;
using DfE.GIAP.Web.Features.MyPupils.PresentationService;
using DfE.GIAP.Web.Features.MyPupils.SelectionState;
using DfE.GIAP.Web.Features.MyPupils.SelectionState.GetPupilSelections;
using DfE.GIAP.Web.Shared.Session.Abstraction.Command;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MessageLevel = DfE.GIAP.Web.Features.MyPupils.Messaging.MessageLevel;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace DfE.GIAP.Web.Features.MyPupils.Controllers.DeleteMyPupils;

#nullable enable

[Route(Routes.MyPupilList.DeleteMyPupils)]
public class DeleteMyPupilsController : Controller
{
    private readonly ILogger<DeleteMyPupilsController> _logger;
    private readonly MyPupilsMessagingOptions _loggingOptions;
    private readonly MyPupilsOptions _options;
    private readonly IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest> _deletePupilsFromMyPupilsUseCase;
    private readonly IGetMyPupilsPupilSelectionProvider _getMyPupilsSelectionState;
    private readonly ISessionCommandHandler<MyPupilsPupilSelectionState> _selectionStateSessionCommandHandler;
    private readonly IMyPupilsMessageSink _myPupilsLogSink;
    private readonly IMyPupilsPresentationService _myPupilsPresentationService;

    public DeleteMyPupilsController(
        ILogger<DeleteMyPupilsController> logger,
        IOptions<MyPupilsOptions> options,
        IOptions<MyPupilsMessagingOptions> loggingOptions,
        IMyPupilsMessageSink myPupilsMessageSink,
        IMyPupilsPresentationService myPupilsPresentationService,
        IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest> deletePupilsUseCase,
        IGetMyPupilsPupilSelectionProvider getMyPupilsStateProvider,
        ISessionCommandHandler<MyPupilsPupilSelectionState> selectionStateCommandHandler)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;

        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(options.Value);
        _options = options.Value;

        ArgumentNullException.ThrowIfNull(loggingOptions);
        ArgumentNullException.ThrowIfNull(loggingOptions.Value);
        _loggingOptions = loggingOptions.Value;

        ArgumentNullException.ThrowIfNull(myPupilsMessageSink);
        _myPupilsLogSink = myPupilsMessageSink;

        ArgumentNullException.ThrowIfNull(myPupilsPresentationService);
        _myPupilsPresentationService = myPupilsPresentationService;

        ArgumentNullException.ThrowIfNull(deletePupilsUseCase);
        _deletePupilsFromMyPupilsUseCase = deletePupilsUseCase;

        ArgumentNullException.ThrowIfNull(getMyPupilsStateProvider);
        _getMyPupilsSelectionState = getMyPupilsStateProvider;

        ArgumentNullException.ThrowIfNull(selectionStateCommandHandler);
        _selectionStateSessionCommandHandler = selectionStateCommandHandler;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete([FromForm] List<string> SelectedPupils, MyPupilsQueryRequestDto queryRequest)
    {
        _logger.LogInformation("{Controller}.{Action} POST method called", nameof(DeleteMyPupilsController), nameof(Delete));

        SelectedPupils ??= [];

        if (!ModelState.IsValid)
        {
            _myPupilsLogSink.Add(
                MyPupilsMessage.Create(
                    MessageLevel.Error,
                    "There has been a problem with your delete selections. Please try again."));

            return RedirectToGetMyPupils(queryRequest);
        }

        MyPupilsPupilSelectionState selectionState = _getMyPupilsSelectionState.GetPupilSelections();

        if (SelectedPupils.Count == 0 && !selectionState.IsAnyPupilSelected)
        {
            _myPupilsLogSink.Add(
                MyPupilsMessage.Create(
                    MessageLevel.Error,
                    Messages.Common.Errors.NoPupilsSelected));

            return RedirectToGetMyPupils(queryRequest);
        }

        string userId = User.GetUserId();

        await _myPupilsPresentationService.DeletePupilsAsync(userId, SelectedPupils);

        _myPupilsLogSink.Add(
            MyPupilsMessage.Create(
                id: _loggingOptions.DeleteSuccessfulKey,
                MessageLevel.Info,
                $"Selected MyPupils were deleted from user: {userId}."));

        return RedirectToGetMyPupils(queryRequest);
    }

    private RedirectToActionResult RedirectToGetMyPupils(MyPupilsQueryRequestDto request)
    {
        return RedirectToAction(
            actionName: "Index",
            controllerName: "GetMyPupils",
            new
            {
                request.PageNumber,
                request.SortField,
                request.SortDirection
            });
    }
}
