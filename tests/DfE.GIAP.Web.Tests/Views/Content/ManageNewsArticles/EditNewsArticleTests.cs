using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using DfE.GIAP.Web.ViewModels;
using DfE.GIAP.Web.ViewModels.Content.ManageNewsArticles;

namespace DfE.GIAP.Web.Tests.Views.Content.ManageNewsArticles;

public sealed class EditNewsArticleTests : IClassFixture<HostFixture>
{
    private const string ViewPath = "/Views/Content/ManageNewsArticles/EditNewsArticle.cshtml";
    private readonly HostFixture _hostFixture;

    public EditNewsArticleTests(HostFixture hostFixture) => _hostFixture = hostFixture;

    [Fact]
    public async Task EditNewsArticle_WithArticle_RendersBoundFieldsAndActions()
    {
        DateTime createdDate = new(2025, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        DateTime modifiedDate = new(2025, 2, 3, 4, 5, 6, DateTimeKind.Utc);
        EditNewsArticleViewModel model = new()
        {
            SelectedNewsId = "selected-id",
            BackButton = new BackButtonViewModel(true, "ManageNewsArticles", "ManageNewsArticles"),
            NewsArticle = new NewsArticleViewModel
            {
                Id = "article-id",
                Title = "Article title",
                Body = "Article body",
                Pinned = true,
                Published = true,
                CreatedDate = createdDate,
                ModifiedDate = modifiedDate
            }
        };

        IHtmlDocument document = await _hostFixture.RenderViewAsync(ViewPath, model);

        Assert.Equal("Edit news article", document.QuerySelector("h1")?.TextContent.Trim());
        Assert.Equal("selected-id", document.QuerySelector("input[name='SelectedNewsId']")?.GetAttribute("value"));
        Assert.Equal("article-id", document.QuerySelector("input[name='NewsArticle.Id']")?.GetAttribute("value"));
        Assert.False(string.IsNullOrWhiteSpace(document.QuerySelector("input[name='NewsArticle.CreatedDate']")?.GetAttribute("value")));
        Assert.False(string.IsNullOrWhiteSpace(document.QuerySelector("input[name='NewsArticle.ModifiedDate']")?.GetAttribute("value")));
        Assert.Equal("Article title", document.QuerySelector("textarea[name='NewsArticle.Title']")?.TextContent.Trim());
        Assert.Equal("Article body", document.QuerySelector("textarea[name='NewsArticle.Body']")?.TextContent.Trim());
        Assert.NotNull(document.QuerySelector("form input[name='NewsArticle.Pinned'][value='true']"));
        Assert.NotNull(document.QuerySelector("form input[name='NewsArticle.Published'][value='true']"));

        IElement form = Assert.Single(document.QuerySelectorAll("form"));
        Assert.Equal("post", form.GetAttribute("method"));
        Assert.Equal("Update article", document.QuerySelector("button[name='update']")?.TextContent.Trim());
        Assert.Equal("Delete article", document.QuerySelector("button[name='delete']")?.TextContent.Trim());
        Assert.EndsWith("/content/manage-news-articles/delete",
            document.QuerySelector("button[name='delete']")?.GetAttribute("formaction"));
        Assert.NotNull(document.QuerySelector("#back-link--link"));
    }

    [Fact]
    public async Task EditNewsArticle_WithoutArticle_DoesNotRenderForm()
    {
        EditNewsArticleViewModel model = new()
        {
            BackButton = new BackButtonViewModel(true, "ManageNewsArticles", "ManageNewsArticles")
        };

        IHtmlDocument document = await _hostFixture.RenderViewAsync(ViewPath, model);

        Assert.Equal("Edit news article", document.QuerySelector("h1")?.TextContent.Trim());
        Assert.Null(document.QuerySelector("form"));
        Assert.Null(document.QuerySelector("button[name='update']"));
        Assert.Null(document.QuerySelector("button[name='delete']"));
    }
}
