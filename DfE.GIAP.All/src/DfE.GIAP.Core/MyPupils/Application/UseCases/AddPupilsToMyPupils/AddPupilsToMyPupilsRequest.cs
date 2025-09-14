using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.Users.Application;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.AddPupilsToMyPupils;
public record AddPupilsToMyPupilsRequest : IUseCaseRequest
{
    public AddPupilsToMyPupilsRequest(
        UserId userId,
        IEnumerable<UniquePupilNumber> pupils,
        CancellationToken ctx = default) // TODO Note not currently on the HandleAsync contract should we bundle on there or as part of request ports?
    {
        ArgumentNullException.ThrowIfNull(pupils);

        Pupils = pupils.Distinct()
                .ToList()
                .AsReadOnly();

        UserId = userId;

        CancellationToken = ctx;
    }

    public UserId UserId { get; }
    public IReadOnlyList<UniquePupilNumber> Pupils { get; }
    public CancellationToken CancellationToken { get; }
}
