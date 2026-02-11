namespace DfE.GIAP.Core.Downloads.Application.Availability.Access.Policies;

public interface IAuthorisationContext
{
    public bool IsAdminUser { get; }
    public bool IsDfeUser { get; }
    public bool IsEstablishment { get; }
    public bool IsLAUser { get; }
    public bool IsMatUser { get; }
    public bool IsSatUser { get; }
    public bool AnyAgeUser { get; }
    int StatutoryAgeLow { get; }
    int StatutoryAgeHigh { get; }
    IReadOnlyCollection<string> Claims { get; }
}
