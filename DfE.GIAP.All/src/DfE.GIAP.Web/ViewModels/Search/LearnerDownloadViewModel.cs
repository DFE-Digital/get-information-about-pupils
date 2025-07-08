using DfE.GIAP.Common.Enums;
using DfE.GIAP.Core.Models.Search;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Web.ViewModels.Search
{
    [ExcludeFromCodeCoverage]
    public class LearnerDownloadViewModel : BaseSearchViewModel
    {
        public List<SearchDownloadDataType> SearchDownloadDatatypes { get; set; } = new List<SearchDownloadDataType>();
        public string LearnerNumber { get; set; }
        public string SelectedPupils { get; set; }
        public string[] SelectedDownloadOptions { get; set; }
        public DownloadFileType DownloadFileType { get; set; }
        public int SelectedPupilsCount { get; set; }
        public LearnerNumberSearchViewModel NumberSearchViewModel { get; set; } = new LearnerNumberSearchViewModel();
        public LearnerTextSearchViewModel TextSearchViewModel { get; set; } = new LearnerTextSearchViewModel();
        public bool ShowTABDownloadType { get; set; }

    }
}
