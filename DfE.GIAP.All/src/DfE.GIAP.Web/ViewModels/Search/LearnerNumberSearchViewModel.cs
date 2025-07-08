using DfE.GIAP.Common.Validation;
using DfE.GIAP.Domain.Search.Learner;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Web.ViewModels.Search;

[ExcludeFromCodeCoverage]
public class LearnerNumberSearchViewModel
{
    public string PageTitle { get; set; }
    public string PageHeading { get; set; }

    public string SearchAction { get; set; }
    public string InvalidUPNsConfirmationAction { get; set; }
    public string FullTextLearnerSearchController { get; set; }
    public string FullTextLearnerSearchAction { get; set; }


    public bool ShowMiddleNames { get; set; }
    public string DownloadLinksPartial { get; set; }
    public bool ShowLocalAuthority { get; set; }

    public string LearnerNumberLabel { get; set; }
    public static int MaximumLearnerNumbersPerSearch { get; set; }

    [SearchLearnerNumberValidation("MaximumLearnerNumbersPerSearch")]
    [DataType(DataType.MultilineText)]
    public string LearnerNumber { get; set; }
    public string LearnerNumberIds { get; set; }
    public string LearnerIdSearchResult { get; set; }
    public IEnumerable<Learner> Learners { get; set; } = new List<Learner>();
    public string AddSelectedToMyPupilListLink { get; set; }
    public string DownloadSelectedASCTFLink { get; set; }
    public string DownloadSelectedLink { get; set; }
    public CommonResponseBodyViewModel NewsPublication { get; set; }
    public bool ItemAddedToMyPupilList { get; set; }
    public bool NoPupilSelected { get; set; }
    public string SelectAllNoJsChecked { get; set; }
    public bool ToggleSelectAll { get; set; } = true;
    public bool ShowErrors { get; set; }
    public bool NoPupil { get; set; }
    public DataReleaseTimeTableViewModel DataReleaseTimeTable { get; set; } = new DataReleaseTimeTableViewModel();
    public string SelectedInvalidUPNOption { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public string SortField { get; set; }
    public string SortDirection { get; set; }
    public int Total { get; set; }
    public string PageLearnerNumbers { get; set; } = "";
    public List<string> SelectedPupil { get; set; }
    public List<string> Duplicates { get; set; } = new List<string>();
    public List<string> Invalid { get; set; } = new List<string>();
    public List<string> NotFound { get; set; } = new List<string>();
    public string ErrorDetails { get; set; }
    public string SearchBoxErrorMessage { get; set; }
}
