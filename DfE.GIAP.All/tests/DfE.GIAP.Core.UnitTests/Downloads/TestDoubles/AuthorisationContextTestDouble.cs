using DfE.GIAP.Core.Downloads.Application.Datasets.Access.Policies;

namespace DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

public static class AuthorisationContextTestDouble
{
    public static IAuthorisationContext Create(
        bool isAdminUser = false,
        bool isDfeUser = false,
        bool isEstablishment = false,
        bool isLAUser = false,
        bool isMatUser = false,
        bool isSatUser = false,
        bool AnyAgeUser = false,
        int statutoryAgeLow = 0,
        int statutoryAgeHigh = 0,
        IReadOnlyCollection<string>? claims = null)
    {
        return new StubAuthorisationContext
        {
            IsAdminUser = isAdminUser,
            IsDfeUser = isDfeUser,
            IsEstablishment = isEstablishment,
            IsLAUser = isLAUser,
            IsMatUser = isMatUser,
            IsSatUser = isSatUser,
            AnyAgeUser = AnyAgeUser,
            StatutoryAgeLow = statutoryAgeLow,
            StatutoryAgeHigh = statutoryAgeHigh,
            Claims = claims ?? Array.Empty<string>()
        };
    }

    private sealed class StubAuthorisationContext : IAuthorisationContext
    {
        public bool IsAdminUser { get; init; }
        public required bool IsDfeUser { get; init; }
        public bool IsEstablishment { get; init; }
        public bool IsLAUser { get; init; }
        public bool IsMatUser { get; init; }
        public bool IsSatUser { get; init; }
        public bool AnyAgeUser { get; init; }
        public required int StatutoryAgeLow { get; init; }
        public required int StatutoryAgeHigh { get; init; }
        public required IReadOnlyCollection<string> Claims { get; init; }
    }
}
