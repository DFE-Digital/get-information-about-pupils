using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Web.Config;

[ExcludeFromCodeCoverage]
public class SecurityHeadersOptions
{
    public const string SectionName = "SecurityHeaders";

    public List<string> Remove { get; set; } = new();
    public Dictionary<string, string> Add { get; set; } = new();
}
