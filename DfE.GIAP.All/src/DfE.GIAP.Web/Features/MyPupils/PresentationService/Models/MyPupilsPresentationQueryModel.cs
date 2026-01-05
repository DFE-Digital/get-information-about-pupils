namespace DfE.GIAP.Web.Features.MyPupils.PresentationService.Models;

public record MyPupilsPresentationQueryModel
{
    public MyPupilsPresentationQueryModel(int pageNumber, string sortBy, string sortDirection)
    {
        Page = new PageNumber(pageNumber);
        Sort = new SortOptions(
            sortBy ?? string.Empty,
            sortDirection ?? string.Empty);
    }

    public PageNumber Page { get; }
    public SortOptions Sort { get; }
    public int PageSize { get; } = 20;
    
    public static MyPupilsPresentationQueryModel CreateDefault() => new(1, string.Empty, string.Empty);
}
