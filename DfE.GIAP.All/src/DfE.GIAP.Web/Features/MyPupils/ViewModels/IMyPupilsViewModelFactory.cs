using DfE.GIAP.Web.Features.MyPupils.Services.GetMyPupilsForUser.ViewModels;
using DfE.GIAP.Web.Features.MyPupils.State;

namespace DfE.GIAP.Web.Features.MyPupils.ViewModel;

public interface IMyPupilsViewModelFactory
{
    MyPupilsViewModel CreateViewModel(
        MyPupilsState state,
        PupilsViewModel pupils,
        MyPupilsViewModelContext context);
}

public record MyPupilsViewModelContext
{
    public MyPupilsViewModelContext(string error = "", bool isDeletePupilsSucessful = false)
    {
        Error = string.IsNullOrEmpty(error) ? string.Empty : error;
        isDeletePupilsSucessful = IsDeletePupilsSuccessful;
    }

    public string Error { get; init; }
    public bool IsDeletePupilsSuccessful { get; init; }

    public static MyPupilsViewModelContext Default() => new();
    public static MyPupilsViewModelContext CreateWithErrorMessage(string error) => new(error);
}
