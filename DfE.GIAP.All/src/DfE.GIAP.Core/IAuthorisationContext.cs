namespace DfE.GIAP.Core;
public interface IAuthorisationContext
{
    string UserId { get; }
    int LowAge { get; }
    int HighAge { get; }
    bool IsAdministrator { get; }
}
