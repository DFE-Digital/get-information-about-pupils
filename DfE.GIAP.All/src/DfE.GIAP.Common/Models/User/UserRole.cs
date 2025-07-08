using System;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Domain.Models.User
{
    [ExcludeFromCodeCoverage]
    public class UserRole
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
    }
}
