using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Web.Features.MyPupils.Areas.GetMyPupils;
using DfE.GIAP.Web.Features.MyPupils.Messaging;
using DfE.GIAP.Web.Features.MyPupils.PresentationService;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Web.Features.MyPupils.Controllers.GetMyPupils;

public sealed class MapMyPupilsPresentationResponseModelToViewModel : IMapper<MyPupilsPresentationResponse, MyPupilsViewModel>
{
    private readonly MyPupilsMessagingOptions _loggingOptions;
    private readonly IMyPupilsMessageSink _myPupilsLogSink;

    public MapMyPupilsPresentationResponseModelToViewModel(
        IMyPupilsMessageSink myPupilsLogSink,
        IOptionsSnapshot<MyPupilsMessagingOptions> loggingOptions)
    {
        ArgumentNullException.ThrowIfNull(myPupilsLogSink);
        _myPupilsLogSink = myPupilsLogSink;

        ArgumentNullException.ThrowIfNull(loggingOptions);
        ArgumentNullException.ThrowIfNull(loggingOptions.Value);
        _loggingOptions = loggingOptions.Value;
    }

    public MyPupilsViewModel Map(MyPupilsPresentationResponse source)
    {
        IReadOnlyList<MyPupilsMessage> logs = _myPupilsLogSink.GetMessages();

        return new MyPupilsViewModel
        {
            CurrentPageOfPupils = source.MyPupils,
            PageNumber = source.PageNumber,
            LastPageNumber = source.TotalPages,
            SortDirection = source.SortedDirection,
            SortField = source.SortedField,
            IsAnyPupilsSelected = source.IsAnyPupilsSelected,
            IsDeleteSuccessful = logs.Any(t => t.Id.Equals(_loggingOptions.DeleteSuccessfulKey)),
            Error = logs.FirstOrDefault(t => t.Level == MessageLevel.Error)?.Message ?? string.Empty
        };
    }
}
