using DfE.GIAP.Web.Features.MyPupils.Services.GetMyPupilsForUser.ViewModels;
using DfE.GIAP.Web.Features.MyPupils.State;

namespace DfE.GIAP.Web.Features.MyPupils.ViewModel;

public interface IMyPupilsViewModelFactory
{
    MyPupilsViewModel CreateViewModel(
        MyPupilsState state,
        PupilsViewModel pupils,
        string? error = "",
        bool isDeleteSuccessful = false);
}
