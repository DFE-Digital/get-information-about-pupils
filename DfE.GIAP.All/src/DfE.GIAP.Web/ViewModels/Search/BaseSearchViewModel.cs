using DfE.GIAP.Common.Enums;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Web.ViewModels.Search
{
    [ExcludeFromCodeCoverage]
    public class BaseSearchViewModel
    {
        public string ErrorDetails { get; set; }
        public bool IsDownloadSelectedNPDData { get; set; }
        public string SearchController { get; set; }
        public string SearchAction { get; set; }
        public string SearchResultPageHeading { get; set; }
        public string SearchNonUpnAction { get; set; }
        public string SearchWithUpnAction { get; set; }
        public string SearchType { get; set; }
        public string SearchUlnAction { get; set; }
        public string SearchUlnController { get; set; }
        public string SearchNonUlnAction { get; set; }
        public string DownloadRoute { get; set; }
        public string RedirectRoute { get; set; }
    }
}
