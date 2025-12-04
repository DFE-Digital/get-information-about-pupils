using DfE.GIAP.Web.Features.MyPupils.GetPupilViewModels;
using DfE.GIAP.Web.Features.MyPupils.State;
using DfE.GIAP.Web.Features.MyPupils.ViewModel;

namespace DfE.GIAP.Web.Features.MyPupils.ViewModels.Factory;

public interface IMyPupilsViewModelFactory
{
    ViewModel.MyPupilsViewModel CreateViewModel(
        MyPupilsState state,
        GetPupilViewModels.MyPupilsPresentationModel pupils,
        MyPupilsViewModelContext context);
}
