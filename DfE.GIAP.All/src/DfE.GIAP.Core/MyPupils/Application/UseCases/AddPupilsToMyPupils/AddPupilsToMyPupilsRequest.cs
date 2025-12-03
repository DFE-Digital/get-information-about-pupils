using DfE.GIAP.Core.Common.Application;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.AddPupilsToMyPupils;
public record AddPupilsToMyPupilsRequest : IUseCaseRequest
{
    public AddPupilsToMyPupilsRequest(
        string userId,
        IEnumerable<string> pupils)
    {
        UserId = userId ?? string.Empty;

        UniquePupilNumbers =
            (pupils?.Distinct()
                .ToList() ?? [])
                .AsReadOnly();
    }

    public string UserId { get; }
    public IReadOnlyList<string> UniquePupilNumbers { get; }
}
