using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Web.Config;

[ExcludeFromCodeCoverage]
public sealed class SessionOptions
{
    public const string SectionName = "SessionOptions";
    public bool IsSessionIdStoredInCookie { get; set; }
}
