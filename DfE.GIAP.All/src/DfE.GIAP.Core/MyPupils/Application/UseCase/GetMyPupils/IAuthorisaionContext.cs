namespace DfE.GIAP.Core.MyPupils.Application.UseCase.GetMyPupils;
public interface IAuthorisationContext
{
    string UserId { get; }
    int LowAge { get; }
    int HighAge { get; }
    bool IsAdministrator { get; }
}
