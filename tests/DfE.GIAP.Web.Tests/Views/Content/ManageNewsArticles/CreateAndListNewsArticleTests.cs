using AngleSharp.Html.Dom;
using DfE.GIAP.Web.ViewModels;
using DfE.GIAP.Web.ViewModels.Content.ManageNewsArticles;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DfE.GIAP.Web.Tests.Views.Content.ManageNewsArticles;

public sealed class CreateAndListNewsArticleTests : IClassFixture<HostFixture>
{
    private readonly HostFixture _hostFixture;

    public CreateAndListNewsArticleTests(HostFixture hostFixture) => _hostFixture = hostFixture;

    [Fact]
    public async Task CreateNewsArticle_RendersFieldsAndSubmitAction()
    {
        CreateNewsArticleViewModel model = new()
        {
            BackButton = new BackButtonViewModel(),
            NewsArticle = new NewsArticleViewModel
            {
                Title = "New article",
                Body = "News content"
            }
        };

        IHtmlDocument document = await _hostFixture.RenderViewAsync(
            "/Views/Content/ManageNewsArticles/CreateNewsArticle.cshtml", model);

        Assert.Equal("New article", document.QuerySelector("textarea[name='NewsArticle.Title']")?.TextContent.Trim());
        Assert.Equal("News content", document.QuerySelector("textarea[name='NewsArticle.Body']")?.TextContent.Trim());
        Assert.Equal("post", document.QuerySelector("form")?.GetAttribute("method"));
        Assert.Equal("Save article", document.QuerySelector("button[name='Create']")?.TextContent.Trim());
        Assert.EndsWith("/content/manage-news-articles/create",
            document.QuerySelector("button[name='Create']")?.GetAttribute("formaction"));
    }

    [Fact]
    public async Task ManageNewsArticles_WithArticle_RendersArticleInList()
    {
        ManageNewsArticlesViewModel model = NewListModel(
            new { Id = "news-123", Name = "School update | 20/11/2026 | Published" });

        IHtmlDocument document = await _hostFixture.RenderViewAsync(
            "/Views/Content/ManageNewsArticles/ManageNewsArticles.cshtml", model);

        Assert.Equal("School update | 20/11/2026 | Published",
            document.QuerySelector("[data-testid='news-article-list'] option[value='news-123']")?.TextContent.Trim());
        Assert.Equal("Select", document.QuerySelector("button[name='select']")?.TextContent.Trim());
        Assert.Null(document.QuerySelector("[data-testid='news-article-list'] option[value='missing']"));
    }

    [Fact]
    public async Task ManageNewsArticles_WithoutArticles_RendersEmptyState()
    {
        ManageNewsArticlesViewModel model = NewListModel(Array.Empty<object>());

        IHtmlDocument document = await _hostFixture.RenderViewAsync(
            "/Views/Content/ManageNewsArticles/ManageNewsArticles.cshtml", model);

        Assert.Contains("No news articles available", document.Body?.TextContent);
        Assert.Null(document.QuerySelector("[data-testid='news-article-list']"));
        Assert.NotNull(document.QuerySelector("#create-new-article--button"));
    }

    private static ManageNewsArticlesViewModel NewListModel(object item) => NewListModel(new[] { item });

    private static ManageNewsArticlesViewModel NewListModel(IEnumerable<object> items) => new()
    {
        SelectedNewsId = string.Empty,
        NewsArticleList = new SelectList(items, "Id", "Name"),
        BackButton = new BackButtonViewModel()
    };
}
