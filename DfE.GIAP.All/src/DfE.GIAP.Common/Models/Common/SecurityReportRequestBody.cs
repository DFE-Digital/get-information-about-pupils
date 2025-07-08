using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Domain.Models.Common
{
    [ExcludeFromCodeCoverage]
    public class SecurityReportRequestBody
    {
        public string SearchType { get; set; }
        public string SearchParameter { get; set; }
        public bool IsFe { get; set; }
    }
}