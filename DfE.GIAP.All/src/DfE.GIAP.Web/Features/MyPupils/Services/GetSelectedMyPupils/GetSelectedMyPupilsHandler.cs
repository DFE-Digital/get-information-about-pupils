using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Web.Features.MyPupils.State.Selection;
using DfE.GIAP.Web.Session.Abstraction.Query;

namespace DfE.GIAP.Web.Features.MyPupils.Services.GetSelectedMyPupils;

internal sealed class GetSelectedMyPupilsHandler : IGetSelectedMyPupilsHandler
{
    private readonly ISessionQueryHandler<MyPupilsPupilSelectionState> _selectionStateSessionQueryHandler;

    public GetSelectedMyPupilsHandler(ISessionQueryHandler<MyPupilsPupilSelectionState> selectionStateSessionQueryHandler)
    {
        ArgumentNullException.ThrowIfNull(selectionStateSessionQueryHandler);
        _selectionStateSessionQueryHandler = selectionStateSessionQueryHandler;
    }

    public IEnumerable<string> GetSelectedMyPupils()
    {
        SessionQueryResponse<MyPupilsPupilSelectionState> sessionQueryResponse = _selectionStateSessionQueryHandler.GetSessionObject();

        if (!sessionQueryResponse.HasValue)
        {
            return [];
        }

        IEnumerable<string> selectedPupils =
            sessionQueryResponse.Value.GetPupilsWithSelectionState()
                .Where(t => t.Value)
                .Select(t => t.Key);

        return selectedPupils;
    }
}
