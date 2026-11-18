using AngleSharp.Html.Dom;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.ViewModels.Search;

namespace DfE.GIAP.Web.Tests.Views.LearnerNumber;

public sealed class SearchResultsTests : IClassFixture<HostFixture>
{
    private const string ViewDirectory = "/Views/Shared/LearnerNumber/";
    private readonly HostFixture _hostFixture;

    public SearchResultsTests(HostFixture hostFixture) => _hostFixture = hostFixture;

    [Theory]
    [InlineData(false, "Select all pupils", "true")]
    [InlineData(true, "Deselect all pupils", "false")]
    public async Task SearchResults_RendersSelectionAndErrorTables(bool selectAll, string buttonText, string buttonValue)
    {
        LearnerNumberSearchViewModel model = NewModel();
        model.ToggleSelectAll = selectAll;
        model.Invalid.Add("BAD");
        model.NotFound.Add("MISSING");
        model.Duplicates.Add("DUPLICATE");
        model.Learners = [new()];
        model.Total = 1;

        IHtmlDocument document = await _hostFixture.RenderViewAsync(ViewDirectory + "_SearchResults.cshtml", model);

        Assert.Contains(buttonText, document.QuerySelector("button[name=SelectAllNoJsChecked]")?.TextContent);
        Assert.Equal(buttonValue, document.QuerySelector("button[name=SelectAllNoJsChecked]")?.GetAttribute("value"));
        Assert.NotNull(document.QuerySelector("#pupil--list--table-invalid"));
        Assert.NotNull(document.QuerySelector("#pupil--list--table-not-found"));
        Assert.NotNull(document.QuerySelector("#pupil--list--table-duplicates"));
        Assert.NotNull(document.QuerySelector(".result-table"));
        Assert.NotNull(document.QuerySelector(".govuk-warning-text"));
    }

    [Fact]
    public async Task SearchResults_ShowsErrorsAndHidesLearnerTable()
    {
        LearnerNumberSearchViewModel model = NewModel();
        model.ShowErrors = true;
        model.NotFound.Add("MISSING");
        model.Learners = [new()];
        model.Total = 1;

        IHtmlDocument document = await _hostFixture.RenderViewAsync(ViewDirectory + "_SearchResults.cshtml", model);

        Assert.NotNull(document.QuerySelector(".error-table.display-results"));
        Assert.NotNull(document.QuerySelector(".result-table.hide-for-non-js"));
        Assert.Contains("Show valid UPNs", document.QuerySelector("button[name=ShowErrors]")?.TextContent);
    }

    [Fact]
    public async Task SearchResults_ShowsSuccessAndDownloadLimit()
    {
        LearnerNumberSearchViewModel model = NewModel();
        model.ItemAddedToMyPupilList = true;
        IHtmlDocument success = await _hostFixture.RenderViewAsync(ViewDirectory + "_SearchResults.cshtml", model);
        Assert.Contains("added to your", success.QuerySelector(".govuk-inset-text")?.TextContent);

        model.ErrorDetails = Messages.Downloads.Errors.UPNLimitExceeded;
        IHtmlDocument limit = await _hostFixture.RenderViewAsync(ViewDirectory + "_SearchResults.cshtml", model);
        Assert.Null(limit.QuerySelector(".govuk-inset-text"));
        Assert.Contains(model.ErrorDetails, limit.QuerySelector("#results-download-CTF-Limit-errormessage")?.TextContent);
    }

    private static LearnerNumberSearchViewModel NewModel() => new()
    {
        LearnerNumberLabel = "UPN",
        SearchAction = "Search",
        FullTextLearnerSearchController = "SearchByName",
        FullTextLearnerSearchAction = "Index",
        DownloadLinksPartial = "~/Views/Shared/_CTFLimitError.cshtml",
        PageSize = 10
    };
}
