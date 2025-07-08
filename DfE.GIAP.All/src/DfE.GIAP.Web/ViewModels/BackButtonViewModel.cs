namespace DfE.GIAP.Web.ViewModels
{
    public class BackButtonViewModel
    {
        public bool IsBackButtonEnabled { get; set; } = false;
        public string PreviousController { get; set; } = string.Empty;
        public string PreviousAction { get; set; } = string.Empty;

        public BackButtonViewModel()
        {

        }

        public BackButtonViewModel(bool isBackButtonEnabled, string previousController, string previousAction)
        {
            IsBackButtonEnabled = isBackButtonEnabled;
            PreviousController = previousController;
            PreviousAction = previousAction;
        }
    }
}
