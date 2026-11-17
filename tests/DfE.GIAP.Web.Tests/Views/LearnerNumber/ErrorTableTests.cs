using AngleSharp.Html.Dom;
using DfE.GIAP.Web.ViewModels.Search;

namespace DfE.GIAP.Web.Tests.Views.LearnerNumber;

public sealed class ErrorTableTests : IClassFixture<HostFixture>
{
    private const string ViewDirectory = "/Views/Shared/LearnerNumber/";
    private readonly HostFixture _hostFixture;

    public ErrorTableTests(HostFixture hostFixture) => _hostFixture = hostFixture;

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ErrorTable_RendersHeadingNumbersAndVisibility(bool showErrors)
    {
        LearnerNumberErrorTableViewModel model = new("Invalid UPNs", ["A123", "B456"], "invalid", showErrors);

        IHtmlDocument document = await _hostFixture.RenderViewAsync(ViewDirectory + "_ErrorTable.cshtml", model);

        Assert.Equal("Invalid UPNs", document.QuerySelector("th")?.TextContent.Trim());
        Assert.Equal(["A123", "B456"], document.QuerySelectorAll("tbody tr td").Select(cell => cell.TextContent.Trim()));
        Assert.NotNull(document.QuerySelector("#pupil--list--table-invalid"));
        Assert.Equal(showErrors, document.QuerySelector(".error-table")?.ClassList.Contains("display-results"));
    }
}
