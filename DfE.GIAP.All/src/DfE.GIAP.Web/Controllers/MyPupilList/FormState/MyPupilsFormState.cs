using DfE.GIAP.Web.Controllers.MyPupilList.ViewModel;

namespace DfE.GIAP.Web.Controllers.MyPupilList.FormState;

public record MyPupilsFormState
{
    private readonly int _pageNumber;
    private readonly string _sortField;
    private readonly string _sortDirection;
    private readonly IEnumerable<string> _selectedPupils;
    private readonly bool _selectAll;
    

    public MyPupilsFormState()
    {
        _pageNumber = 1;
        _sortField = string.Empty;
        _sortDirection = string.Empty;
        _selectedPupils = [];
        _selectAll = false;
    }

    public MyPupilsFormState(
        int pageNumber,
        string sortField,
        string sortDirection,
        IEnumerable<string> SelectedPupils,
        bool SelectAll)
    {
        _pageNumber = pageNumber;
        _sortField = sortField ?? string.Empty;
        _sortDirection = sortDirection ?? string.Empty;
        _selectedPupils = SelectedPupils ?? [];
        _selectAll = SelectAll;
    }

    public string SortBy => _sortField;

    public SortDirection SortDirection => _sortDirection switch
    {
        "asc" => SortDirection.Ascending,
        _ => SortDirection.Descending
    };

    public PageNumber Page => PageNumber.Page(_pageNumber);
    public bool SelectAll => _selectAll;
    public IEnumerable<string> SelectedPupils => _selectedPupils;
    public bool NoSelectionsForPupilsMade => !_selectAll && !_selectedPupils.Any();
}
