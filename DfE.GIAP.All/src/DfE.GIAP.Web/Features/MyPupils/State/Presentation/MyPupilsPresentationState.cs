using Microsoft.Azure.Cosmos.Serialization.HybridRow.Schemas;

namespace DfE.GIAP.Web.Features.MyPupils.State.Presentation;

public record MyPupilsPresentationState(
    int Page,
    string SortBy,
    SortDirection SortDirection)
{
    public static MyPupilsPresentationState CreateDefault() => new(Page: 1, SortBy: string.Empty, SortDirection: SortDirection.Descending);
}
