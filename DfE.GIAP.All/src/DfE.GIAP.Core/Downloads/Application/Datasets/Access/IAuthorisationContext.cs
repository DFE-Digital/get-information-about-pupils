namespace DfE.GIAP.Core.Downloads.Application.Datasets.Access;

public interface IAuthorisationContext
{
    string Role { get; }
    public bool IsDfeUser { get; }
    int StatutoryAgeLow { get; }
    int StatutoryAgeHigh { get; }
    IReadOnlyCollection<string> Claims { get; }
}
