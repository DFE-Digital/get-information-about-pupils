using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using DfE.GIAP.Common.Enums;

namespace DfE.GIAP.Web.ViewModels.Search
{
    [ExcludeFromCodeCoverage]
    public class StarredPupilConfirmationViewModel
    {
        public string PageTitle { get; set; }
        public string SelectedPupil { get; set; }
        public DownloadType DownloadType { get; set; }
        public bool ConfirmationGiven { get; set; }
        public bool ConfirmationError { get; set; }
        public string ConfirmationReturnController { get; set; }
        public string ConfirmationReturnAction { get; set; }
        public string CancelReturnController { get; set; }
        public string CancelReturnAction { get; set; }
        public string LearnerNumbers { get; set; }
    }
}
