namespace DfE.GIAP.Web.Features.MyPupils.ViewModel;

public record MyPupilsErrorViewModel
{
    private MyPupilsErrorViewModel(string message)
    {
        Message = message?.Trim() ?? string.Empty;
    }
    public bool HasErrorMessage => !string.IsNullOrEmpty(Message);

    public string Message { get; }

    public static MyPupilsErrorViewModel NOOP() => new(string.Empty);
    public static MyPupilsErrorViewModel Create(string message) => new(message);
    
} // TODO add behaviour
