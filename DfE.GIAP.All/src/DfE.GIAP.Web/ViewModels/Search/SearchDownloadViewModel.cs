using DfE.GIAP.Core.Models.Search;
using DfE.GIAP.Common.Enums;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Web.ViewModels.Search
{
    [ExcludeFromCodeCoverage]
    public class SearchDownloadViewModel : BaseSearchViewModel
    {
        public List<SearchDownloadDataType> SearchDownloadDatatypes { get; set; }

        public string Upn { get; set; }
        public string[] SelectedDownloadOptions { get; set; }

        public string[] SelectedPupils { get; set; }

        public string SelectedPupilsJoined { get; set; }

        public string ReturnUpns { get; set; }

        public string ReturnUlns { get; set; }

        public DownloadFileType DownloadFileType { get; set; }

        public bool UPNFlag { get; set; }

        public bool ConfirmationGiven { get; set; }

        public string ConfirmationErrorMessage { get; set; }

        public string SearchText { get; set; }

        public ReturnRoute ReturnRoute { get; set; }

        public int SelectedPupilsCount { get; set; }

        public bool Duplicates { get; set; }
    }
}