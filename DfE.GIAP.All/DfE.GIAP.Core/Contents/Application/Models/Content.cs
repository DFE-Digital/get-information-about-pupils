namespace DfE.GIAP.Core.Contents.Application.Models;

/// <summary>
/// Represents the content associated with a page, including its body and title.
/// </summary>
public record Content
{
    /// <summary>
    /// Gets the body of the content.
    /// </summary>
    public required string Body { get; init; }

    /// <summary>
    /// Gets the title of the content.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Creates an empty <see cref="Content"/> instance with empty body and title.
    /// </summary>
    /// <returns>An empty <see cref="Content"/> object.</returns>
    public static Content Empty() => new()
    {
        Body = string.Empty,
        Title = string.Empty
    };

    /// <summary>
    /// Determines whether the content is empty (both body and title are null or empty).
    /// </summary>
    /// <returns><c>true</c> if both body and title are empty; otherwise, <c>false</c>.</returns>
    public bool IsEmpty() => string.IsNullOrEmpty(Body) && string.IsNullOrEmpty(Title);
}
