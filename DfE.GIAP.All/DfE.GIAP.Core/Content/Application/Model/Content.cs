namespace DfE.GIAP.Core.Content.Application.Model;
public record Content
{
    public required string Body { get; init; }
    public required string Title { get; init; }
}
