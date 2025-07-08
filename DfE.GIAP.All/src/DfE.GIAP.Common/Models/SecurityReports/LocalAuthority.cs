using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Domain.Models.SecurityReports
{
    [ExcludeFromCodeCoverage]
    public class LocalAuthority
    {
        public string Name { get; set; }
        public int Code { get; set; }
        public string Description => $"{Name}: {Code}";
    }
}
