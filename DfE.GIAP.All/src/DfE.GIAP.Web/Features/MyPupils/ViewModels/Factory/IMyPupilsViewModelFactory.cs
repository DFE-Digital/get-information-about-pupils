using DfE.GIAP.Web.Features.MyPupils.Services.GetPupilViewModels;
using DfE.GIAP.Web.Features.MyPupils.State;
using DfE.GIAP.Web.Features.MyPupils.ViewModel;

namespace DfE.GIAP.Web.Features.MyPupils.ViewModels.Factory;

public interface IMyPupilsViewModelFactory
{
    MyPupilsViewModel CreateViewModel(
        MyPupilsState state,
        PupilsViewModel pupils,
        MyPupilsViewModelContext context);
}
