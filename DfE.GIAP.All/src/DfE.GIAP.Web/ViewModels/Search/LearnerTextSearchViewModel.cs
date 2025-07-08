using DfE.GIAP.Common.Enums;
using DfE.GIAP.Core.Models.Search;
using DfE.GIAP.Domain.Search.Learner;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Web.ViewModels.Search;

[ExcludeFromCodeCoverage]
public class LearnerTextSearchViewModel
{
    public LearnerTextSearchViewModel()
    {
        this.SearchFilters = new SearchFilters();
        this.FilterErrors = new FilterErrors();
        this.RedirectUrls = new RedirectUrls();
    }


    public string PageTitle { get; set; }
    public string PageHeading { get; set; }

    public string DownloadLinksPartial { get; set; }

    public string LearnerTextSearchController { get; set; }
    public string LearnerTextSearchAction { get; set; }
    public string LearnerNumberController { get; set; }
    public string LearnerNumberAction { get; set; }
    public string CSVDownloadAction { get; set; }
    public string InvalidUPNsConfirmationAction { get; set; }

    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }
    public string PageLearnerNumbers { get; set; } = "";

    public bool ShowGender { get; set; }
    public bool ShowLocalAuthority { get; set; }

    public bool ShowMiddleNames { get; set; }
    public string LearnerNumberLabel { get; set; }

    [Required(ErrorMessage = "Enter a first name and/or surname")]
    public string SearchText { get; set; }

    public bool ShowErrors { get; set; }
    public bool NoPupil { get; set; }
    public IEnumerable<Learner> Learners { get; set; } = new List<Learner>();
    public int Count { get; set; }
    public string AddSelectedToMyPupilListLink { get; set; }
    public string DownloadSelectedASCTFLink { get; set; }
    public string DownloadSelectedLink { get; set; }

    public List<FilterData> Filters { get; set; }
    public SearchFilters SearchFilters { get; set; }
    public string[] SelectedGenderValues { get; set; }
    public string[] SelectedSexValues { get; set; }
    public FilterErrors FilterErrors { get; set; }
    public int MaximumResults { get; set; }
    public RedirectUrls RedirectUrls { get; set; }
    public string SelectedPupil { get; set; }
    public string LearnerTextDatabaseName { get; set; }
    public ReturnRoute ReturnRoute { get; set; }
    public bool ItemAddedToMyPupilList { get; set; }
    public bool NoPupilSelected { get; set; }
    public DataReleaseTimeTableViewModel DataReleaseTimeTable { get; set; } = new DataReleaseTimeTableViewModel();
    public string RedirectFrom { get; set; }
    public string ErrorDetails { get; set; }
    public string SortDirection { get; set; }
    public string SortField { get; set; }
    public bool ShowOverLimitMessage { get; set; }
    public bool ShowHiddenUPNWarningMessage { get; set; }
    public bool ShowStarredPupilConfirmation { get; set; }
    public StarredPupilConfirmationViewModel StarredPupilConfirmationViewModel { get; set; } = new StarredPupilConfirmationViewModel();
}
