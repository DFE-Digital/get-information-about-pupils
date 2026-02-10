namespace DfE.GIAP.Web.Features.MyPupils.Services.GetPupils;
public record MyPupilsPresentationResponse
{
    public required MyPupilsPresentationPupilModels MyPupils { get; init; }
    public required int PageNumber { get; init; }
    public required bool IsAnyPupilsSelected { get; init; } = false;
    public string SortedDirection { get; init; } = string.Empty;
    public string SortedField { get; init; } = string.Empty;
}
