using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Web.Features.MyPupils.State.Selection;
using DfE.GIAP.Web.Session.Abstraction.Query;

namespace DfE.GIAP.Web.Features.MyPupils.GetSelectedMyPupils;

internal sealed class GetSelectedMyPupilsProvider : IGetSelectedMyPupilsProvider
{
    private readonly ISessionQueryHandler<MyPupilsPupilSelectionState> _selectionStateSessionQueryHandler;

    public GetSelectedMyPupilsProvider(ISessionQueryHandler<MyPupilsPupilSelectionState> selectionStateSessionQueryHandler)
    {
        ArgumentNullException.ThrowIfNull(selectionStateSessionQueryHandler);
        _selectionStateSessionQueryHandler = selectionStateSessionQueryHandler;
    }

    public UniquePupilNumbers GetSelectedMyPupils()
    {
        SessionQueryResponse<MyPupilsPupilSelectionState> sessionQueryResponse = _selectionStateSessionQueryHandler.GetSessionObject();

        if (!sessionQueryResponse.HasValue)
        {
            return UniquePupilNumbers.Create(uniquePupilNumbers: []);
        }

        IEnumerable<UniquePupilNumber> selectedPupils =
            sessionQueryResponse.Value.GetPupilsWithSelectionState()
                .Where(t => t.Value)
                .Select(t => t.Key);

        return UniquePupilNumbers.Create(selectedPupils);
    }
}
