using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Web.ViewModels
{
    [ExcludeFromCodeCoverage]
    public class UserErrorViewModel
    {
        public string UserErrorMessage { get; set; }
        public BackButtonViewModel BackButton { get; set; }
    }
}