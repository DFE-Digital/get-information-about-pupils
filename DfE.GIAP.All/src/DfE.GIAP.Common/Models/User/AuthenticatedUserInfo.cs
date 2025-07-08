using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Domain.Models.User
{
    [ExcludeFromCodeCoverage]
    public class AuthenticatedUserInfo : UserInfo
    {
        public bool IsAdmin { get; set; }
        public bool IsApprover { get; set; }
        public bool IsUser { get; set; }
    }
}
