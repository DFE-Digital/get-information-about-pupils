using DfE.GIAP.Core.Downloads.Application.Datasets.Access.Policies;

namespace DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

public static class AuthorisationContextTestDouble
{
    public static IAuthorisationContext Create(
        string role = "User",
        bool isDfeUser = false,
        int statutoryAgeLow = 0,
        int statutoryAgeHigh = 0,
        IReadOnlyCollection<string>? claims = null)
    {
        return new StubAuthorisationContext
        {
            Role = role,
            IsDfeUser = isDfeUser,
            StatutoryAgeLow = statutoryAgeLow,
            StatutoryAgeHigh = statutoryAgeHigh,
            Claims = claims ?? Array.Empty<string>()
        };
    }

    private sealed class StubAuthorisationContext : IAuthorisationContext
    {
        public required string Role { get; init; }
        public required bool IsDfeUser { get; init; }
        public required int StatutoryAgeLow { get; init; }
        public required int StatutoryAgeHigh { get; init; }
        public required IReadOnlyCollection<string> Claims { get; init; }
    }
}
