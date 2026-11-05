namespace DfE.GIAP.Core;
public readonly struct UserRole
{
    public UserRole(bool isAdminisrator)
    {
        IsAdministrator = isAdminisrator;
    }

    public bool IsAdministrator { get; }
}
