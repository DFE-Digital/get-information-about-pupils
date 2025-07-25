namespace DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Request;
public interface IAuthorisationContext
{
    string UserId { get; }
    int LowAge { get; }
    int HighAge { get; }
    bool IsAdministrator { get; }
}
