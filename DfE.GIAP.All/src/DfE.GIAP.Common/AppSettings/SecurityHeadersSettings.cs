using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Common.AppSettings
{
    [ExcludeFromCodeCoverage]
    public class SecurityHeadersSettings
    {
        public const string SectionName = "SecurityHeaders";

        public List<string> Remove { get; set; } = new();
        public Dictionary<string, string> Add { get; set; } = new();
    }
}
