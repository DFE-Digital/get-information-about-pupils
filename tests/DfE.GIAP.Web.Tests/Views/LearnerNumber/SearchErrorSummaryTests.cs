using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.ViewModels.Search;

namespace DfE.GIAP.Web.Tests.Views.LearnerNumber;

public sealed class SearchErrorSummaryTests : IClassFixture<HostFixture>
{
    private const string ViewDirectory = "/Views/Shared/LearnerNumber/";
    private readonly HostFixture _hostFixture;

    public SearchErrorSummaryTests(HostFixture hostFixture) => _hostFixture = hostFixture;

    [Fact]
    public async Task SearchErrorSummary_HidesWhenThereAreNoErrors()
    {
        IHtmlDocument document = await _hostFixture.RenderViewAsync(ViewDirectory + "_SearchErrorSummary.cshtml", NewModel());

        Assert.Null(document.QuerySelector(".govuk-error-summary"));
    }

    [Theory]
    [InlineData(true, false, false, "You have not selected any pupils")]
    [InlineData(false, true, false, "Enter one or more UPNs")]
    [InlineData(false, false, true, Messages.Common.Errors.MyPupilListLimitExceeded)]
    public async Task SearchErrorSummary_ShowsOnlyTheApplicableError(
        bool noPupilSelected, bool invalidSearch, bool listLimitExceeded, string expectedMessage)
    {
        LearnerNumberSearchViewModel model = NewModel();
        model.NoPupilSelected = noPupilSelected;
        model.SearchBoxErrorMessage = "Enter one or more UPNs";
        model.ErrorDetails = listLimitExceeded ? Messages.Common.Errors.MyPupilListLimitExceeded : null;

        IHtmlDocument document = await _hostFixture.RenderViewAsync(
            ViewDirectory + "_SearchErrorSummary.cshtml", model,
            state => { if (invalidSearch) state.AddModelError(nameof(model.LearnerNumber), model.SearchBoxErrorMessage); });

        Assert.Contains(expectedMessage, Assert.Single(document.QuerySelectorAll(".govuk-error-summary__list a")).TextContent);
    }

    [Fact]
    public async Task SearchErrorSummary_ShowsEachApplicableErrorAndItsTarget()
    {
        LearnerNumberSearchViewModel model = NewModel();
        model.SearchBoxErrorMessage = "Enter one or more UPNs";
        model.NoPupilSelected = true;
        model.ErrorDetails = Messages.Common.Errors.MyPupilListLimitExceeded;

        IHtmlDocument document = await _hostFixture.RenderViewAsync(
            ViewDirectory + "_SearchErrorSummary.cshtml", model,
            state => state.AddModelError(nameof(model.LearnerNumber), model.SearchBoxErrorMessage));

        Assert.Equal("There is a problem", document.QuerySelector(".govuk-error-summary__title")?.TextContent.Trim());
        IHtmlCollection<IElement> links = document.QuerySelectorAll(".govuk-error-summary__list a");
        Assert.Equal(3, links.Length);
        Assert.Equal("#search--textarea", links[0].GetAttribute("href"));
        Assert.Contains(model.SearchBoxErrorMessage, links[0].TextContent);
        Assert.Equal("#pupil--list--table", links[1].GetAttribute("href"));
        Assert.Contains("You have not selected any pupils", links[1].TextContent);
        Assert.Contains(model.ErrorDetails, links[2].TextContent);
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
