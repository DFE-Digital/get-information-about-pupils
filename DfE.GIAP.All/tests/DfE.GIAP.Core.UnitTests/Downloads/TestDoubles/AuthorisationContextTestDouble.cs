using DfE.GIAP.Core.Downloads.Application.Datasets.Access;

namespace DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

public sealed class AuthorisationContextTestDouble : IAuthorisationContext
{
    public string Role { get; init; } = "User";
    public bool IsDfeUser { get; init; } = false;
    public int StatutoryAgeLow { get; init; } = 0;
    public int StatutoryAgeHigh { get; init; } = 0;
    public IReadOnlyCollection<string> Claims { get; init; } = Array.Empty<string>();
}
