using DfE.GIAP.Common.Enums;
using DfE.GIAP.Core.Models.Search;
using DfE.GIAP.Domain.Models.MPL;
using DfE.GIAP.Domain.Models.Search;
using DfE.GIAP.Domain.Search.Learner;
using DfE.GIAP.Web.ViewModels.Search;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Web.ViewModels;

[ExcludeFromCodeCoverage]
public class MyPupilListViewModel : BaseSearchViewModel
{
    public MyPupilListViewModel()
    {
        Results = new List<UpnResults>();
    }

    public IList<MyPupilListItem> MyPupilList { get; set; }
    public IList<UpnResults> Results { get; set; }

    public IList<PupilDetail> MyPupilListForDisplay { get; set; }

    public string SelectedPupilsJoined { get; set; }

    public ReturnRoute ReturnRoute { get; set; }

    public string SearchText { get; set; }

    public string NonUpnDatabaseName { get; set; }
    public StarredPupilConfirmationViewModel StarredPupilConfirmationViewModel { get; internal set; } = new StarredPupilConfirmationViewModel();

    public bool Removed { get; set; }

    public bool MaxFail { get; set; }

    public bool NoPupilSelectedError { get; set; }


    public string PageTitle { get; set; }
    public string PageHeading { get; set; }


    public string DownloadLinksPartial { get; set; }
    public string RemoveSelectedToMyPupilListLink { get; set; }
    public string DownloadSelectedNPDDataLink { get; set; }
    public string DownloadSelectedPupilPremiumDataLink { get; set; }
    public string DownloadSelectedASCTFLink { get; set; }


    public string LearnerNumberLabel { get; set; }
    public static int MaximumUPNsPerSearch { get; set; }
    public string Upn { get; set; }
    public IEnumerable<Learner> Learners { get; set; } = new List<Learner>();
    public bool NoPupilSelected { get; set; }
    public string SelectAllNoJsChecked { get; set; }
    public bool ToggleSelectAll { get; set; } = false;
    public bool ShowErrors { get; set; }
    public bool Fail { get; set; }
    public bool NoPupil { get; set; }
    public string SelectedInvalidUPNOption { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }
    public string PageLearnerNumbers { get; set; }
    public List<string> SelectedPupil { get; set; }
    public List<Learner> Invalid { get; set; } = new List<Learner>();
    public string SearchBoxErrorMessage { get; set; }
    public bool ShowLocalAuthority { get; set; }
    public string SortField { get; set; }
    public string SortDirection { get; set; }
}
