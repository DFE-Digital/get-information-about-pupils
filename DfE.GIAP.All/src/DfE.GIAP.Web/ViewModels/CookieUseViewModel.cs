using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Web.ViewModels
{
    [ExcludeFromCodeCoverage]
    public class CookieUseViewModel
    {
        public string CookieWebsiteUse { get; set; }

        public string CookieComms { get; set; }

        public bool IsCookieWebsiteUse { get; set; }

        public bool IsCookieComms { get; set; }
    }
}
