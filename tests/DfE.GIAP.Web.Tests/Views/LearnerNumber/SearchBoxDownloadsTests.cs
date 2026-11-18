using AngleSharp.Html.Dom;
using DfE.GIAP.Web.ViewModels.Search;

namespace DfE.GIAP.Web.Tests.Views.LearnerNumber;

public sealed class SearchBoxDownloadsTests : IClassFixture<HostFixture>
{
    private const string ViewDirectory = "/Views/Shared/LearnerNumber/";
    private readonly HostFixture _hostFixture;

    public SearchBoxDownloadsTests(HostFixture hostFixture) => _hostFixture = hostFixture;

    [Fact]
    public async Task SearchBoxDownloads_RendersSearchValuesAndLinks()
    {
        LearnerNumberSearchViewModel model = NewModel();
        model.LearnerNumber = "A123\nB456";

        IHtmlDocument document = await _hostFixture.RenderViewAsync(ViewDirectory + "_SearchBoxDownloads.cshtml", model);

        Assert.Equal(model.LearnerNumber, document.QuerySelector("textarea")?.TextContent.Trim());
        Assert.Contains("UPNs on separate lines", document.QuerySelector("label")?.TextContent);
        Assert.Equal("UPN", document.QuerySelector("#LearnerNumberLabel")?.GetAttribute("value"));
        Assert.NotNull(document.QuerySelector("#search--btn"));
        Assert.NotNull(document.QuerySelector("#link-search-again"));
        Assert.NotNull(document.QuerySelector("#search-searchwithout--link"));
        Assert.Null(document.QuerySelector(".govuk-error-message"));
    }

    [Fact]
    public async Task SearchBoxDownloads_ShowsValidationMessage()
    {
        LearnerNumberSearchViewModel model = NewModel();
        model.SearchBoxErrorMessage = "Enter one or more UPNs";

        IHtmlDocument document = await _hostFixture.RenderViewAsync(
            ViewDirectory + "_SearchBoxDownloads.cshtml", model,
            state => state.AddModelError(nameof(model.LearnerNumber), model.SearchBoxErrorMessage));

        Assert.Contains(model.SearchBoxErrorMessage, document.QuerySelector(".govuk-error-message")?.TextContent);
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
