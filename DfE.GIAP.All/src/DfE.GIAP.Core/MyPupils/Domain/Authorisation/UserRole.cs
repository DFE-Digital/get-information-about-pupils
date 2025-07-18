namespace DfE.GIAP.Core.MyPupils.Domain.Authorisation;
public readonly struct UserRole
{
    public UserRole(bool isAdminisrator)
    {
        IsAdministrator = isAdminisrator;
    }

    public bool IsAdministrator { get; }
}
