using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Web.Features.MyPupils.State.Selection;

namespace DfE.GIAP.Web.Features.MyPupils.Handlers.GetMyPupils.ViewModel.Factory;

public interface IPupilsViewModelFactory
{
    PupilsViewModel CreateViewModel(MyPupilDtos myPupils, MyPupilsPupilSelectionState selectionState);
}
