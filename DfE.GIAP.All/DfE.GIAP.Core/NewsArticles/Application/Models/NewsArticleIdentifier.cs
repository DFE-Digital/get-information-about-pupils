namespace DfE.GIAP.Core.NewsArticles.Application.Models;

public record NewsArticleIdentifier
{
    public string Value { get; }
    private NewsArticleIdentifier(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    public static NewsArticleIdentifier New() => new(Guid.NewGuid().ToString());
    public static NewsArticleIdentifier From(string value) => new(value);
}
