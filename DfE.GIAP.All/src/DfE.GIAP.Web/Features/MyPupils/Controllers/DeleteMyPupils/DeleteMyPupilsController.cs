using DfE.GIAP.Core.MyPupils.Application.Options;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Features.MyPupils.Messaging;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection.GetPupilSelections;
using DfE.GIAP.Web.Features.MyPupils.Services.DeletePupils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MessageLevel = DfE.GIAP.Web.Features.MyPupils.Messaging.MessageLevel;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace DfE.GIAP.Web.Features.MyPupils.Controllers.DeleteMyPupils;

#nullable enable

[Route(Routes.MyPupilList.MyPupilsBase)]
public class DeleteMyPupilsController : Controller
{
    private readonly ILogger<DeleteMyPupilsController> _logger;
    private readonly MyPupilsMessagingOptions _loggingOptions;
    private readonly MyPupilsOptions _options;
    private readonly IGetMyPupilsPupilSelectionProvider _getMyPupilsSelectionState;
    private readonly IMyPupilsMessageSink _myPupilsLogSink;
    private readonly IDeleteMyPupilsPresentationService _deleteService;

    public DeleteMyPupilsController(
        ILogger<DeleteMyPupilsController> logger,
        IOptions<MyPupilsOptions> options,
        IOptions<MyPupilsMessagingOptions> messagingOptions,
        IMyPupilsMessageSink messageSink,
        IDeleteMyPupilsPresentationService deleteService,
        IGetMyPupilsPupilSelectionProvider pupilSelectionStateProvider)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;

        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(options.Value);
        _options = options.Value;

        ArgumentNullException.ThrowIfNull(messagingOptions);
        ArgumentNullException.ThrowIfNull(messagingOptions.Value);
        _loggingOptions = messagingOptions.Value;

        ArgumentNullException.ThrowIfNull(messageSink);
        _myPupilsLogSink = messageSink;

        ArgumentNullException.ThrowIfNull(deleteService);
        _deleteService = deleteService;

        ArgumentNullException.ThrowIfNull(pupilSelectionStateProvider);
        _getMyPupilsSelectionState = pupilSelectionStateProvider;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route(Routes.MyPupilList.DeleteMyPupilsRoute)]
    public async Task<IActionResult> Delete(
        [FromForm] List<string>? SelectedPupils,
        MyPupilsQueryRequestDto? queryRequest)
    {
        _logger.LogInformation("{Controller}.{Action} POST method called", nameof(DeleteMyPupilsController), nameof(Delete));

        SelectedPupils ??= [];
        queryRequest ??= new();

        if (!ModelState.IsValid)
        {
            _myPupilsLogSink.AddMessage(
                MyPupilsMessage.Create(
                    MessageLevel.Error,
                    "There has been a problem with your delete selections. Please try again."));

            return MyPupilsRedirectHelpers.RedirectToGetMyPupils(queryRequest);
        }

        MyPupilsPupilSelectionState selectionState = _getMyPupilsSelectionState.GetPupilSelections();

        if (SelectedPupils.Count == 0 && !selectionState.IsAnyPupilSelected)
        {
            _myPupilsLogSink.AddMessage(
                MyPupilsMessage.Create(
                    MessageLevel.Error,
                    Messages.Common.Errors.NoPupilsSelected));

            return MyPupilsRedirectHelpers.RedirectToGetMyPupils(queryRequest);
        }

        string userId = User.GetUserId();

        await _deleteService.DeletePupilsAsync(userId, SelectedPupils);

        _myPupilsLogSink.AddMessage(
            MyPupilsMessage.Create(
                id: _loggingOptions.DeleteSuccessfulKey,
                MessageLevel.Info,
                $"Selected MyPupils were deleted from user: {userId}."));

        return MyPupilsRedirectHelpers.RedirectToGetMyPupils(queryRequest);
    }
}
