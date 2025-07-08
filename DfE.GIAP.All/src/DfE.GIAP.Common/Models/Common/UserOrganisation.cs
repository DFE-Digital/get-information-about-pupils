using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Domain.Models.Common
{
    [ExcludeFromCodeCoverage]
    public class UserOrganisation
    {
        public bool IsAdmin { get; set; }

        public bool IsEstablishment { get; set; }

        public bool IsLa { get; set; }

        public bool IsMAT { get; set; }

        public bool IsSAT { get; set; }
    }
}
