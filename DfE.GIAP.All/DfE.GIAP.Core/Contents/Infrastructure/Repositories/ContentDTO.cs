namespace DfE.GIAP.Core.Contents.Infrastructure.Repositories;

/// <summary>
/// Data Transfer Object (DTO) representing content stored in Cosmos DB.
/// </summary>
public class ContentDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the content document.
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// Gets or sets the body of the content.
    /// </summary>
    public string? Body { get; set; }

    /// <summary>
    /// Gets or sets the title of the content.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the document type identifier.
    /// </summary>
    public int DOCTYPE { get; set; }
}
