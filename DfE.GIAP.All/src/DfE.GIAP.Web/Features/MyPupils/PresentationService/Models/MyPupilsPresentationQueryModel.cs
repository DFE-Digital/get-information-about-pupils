namespace DfE.GIAP.Web.Features.MyPupils.PresentationService.Models;

public record MyPupilsPresentationQueryModel
{
    public MyPupilsPresentationQueryModel(int pageNumber, int pageSize, string sortBy, string sortDirection)
    {
        Page = new PageNumber(pageNumber);

        Sort = new SortOptions(
            sortBy ?? string.Empty,
            sortDirection ?? string.Empty);

        PageSize = pageSize;
    }

    // TODO PaginateOptions
    public PageNumber Page { get; }
    public int PageSize { get; }
    public SortOptions Sort { get; }
    
    public static MyPupilsPresentationQueryModel CreateDefault() => new(1, 20, string.Empty, string.Empty);
}
