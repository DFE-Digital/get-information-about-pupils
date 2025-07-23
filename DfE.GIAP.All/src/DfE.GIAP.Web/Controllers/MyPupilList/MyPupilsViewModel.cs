using DfE.GIAP.Core.MyPupils.Application.UseCases.Services.AggregatePupilsForMyPupilsDomainService.Dto;
using DfE.GIAP.Domain.Search.Learner;

namespace DfE.GIAP.Web.Controllers.MyPupilList;

public sealed class MyPupilsViewModel
{
    private readonly MyPupilsErrorModel? _error;
    public MyPupilsViewModel(
        IEnumerable<PupilPresentatationModel> pupils,
        MyPupilsErrorModel? error = null)
    {
        Pupils = pupils;
        _error = error;
    }

    public string PageHeading => "My pupil list";

    public IEnumerable<PupilPresentatationModel> Pupils { get; } = [];
    public IEnumerable<Domain.Search.Learner.Learner> InvalidUpns => []; // TODO change to PupilPresentationModel
    /* Old implementation that hydrated
     *     private void SetInvalid(Learner learner, MyPupilListViewModel model, bool isMasked)
            {
                bool isValid = ValidationHelper.IsValidUpn(isMasked ? RbacHelper.DecodeUpn(learner.LearnerNumberId) : learner.LearnerNumber);
                if (!isValid)
                {
                    model.Invalid.Add(learner);
                }
        }
     * 
     */

    public bool IsPreviousPageAvailable => PageNumber > 1;
    public bool IsMorePageAvailable => Pupils.Count() == PageSize;
    public bool NoPupils => Pupils.Count() == 0;

    public int PageNumber { get; set; } = 1;
    public int PageSize => 20;
    public string SortField { get; set; } = string.Empty;
    public string SortDirection { get; set; } = string.Empty;

    public string UniquePupilNumberLabel => "UPN";
    public string FormAction => "MyPupilList";

    public bool isRemovedSuccessful => false;
    public bool NoPupilSelected => false;
    public bool ToggleSelectAll => false;

    public string ErrorMessage => _error?.Message ?? string.Empty;
    public bool IsError => _error is not null;
}
