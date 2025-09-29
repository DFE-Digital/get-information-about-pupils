namespace DfE.GIAP.Web.Features.MyPupils.ViewModels.Factory;

public record MyPupilsViewModelContext
{
    public MyPupilsViewModelContext(string error = "", bool isDeletePupilsSucessful = false)
    {
        Error = string.IsNullOrEmpty(error) ? string.Empty : error;
        IsDeletePupilsSuccessful = isDeletePupilsSucessful;
    }

    public string Error { get; init; }
    public bool IsDeletePupilsSuccessful { get; init; }

    public static MyPupilsViewModelContext Default() => new();
    public static MyPupilsViewModelContext CreateWithErrorMessage(string error) => new(error);
}
