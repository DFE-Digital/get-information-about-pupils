namespace DfE.GIAP.Core.Content.Application.Model;
public record Content
{
    public required string Body { get; init; }
    public required string Title { get; init; }
    public static Content Empty() => new()
    {
        Body = string.Empty,
        Title = string.Empty
    };
    public bool IsEmpty() => string.IsNullOrEmpty(Body) && string.IsNullOrEmpty(Title);
}
