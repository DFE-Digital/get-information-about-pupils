using DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PresenterHandler.Order;

namespace DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PresenterHandler.Options;

public record PresentPupilsOptions(
    int Page,
    string SortBy,
    SortDirection SortDirection)
{
    public static PresentPupilsOptions Default => new(1, string.Empty, SortDirection.Descending);
}
