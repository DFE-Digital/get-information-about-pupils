using DfE.GIAP.Core.Common.Application.TextSanitiser.Abstraction.Handler;

namespace DfE.GIAP.Core.Common.Application.TextSanitiser.Sanitiser;
public sealed class SanitisedTextResult
{
    public string Value { get; }
    // Should not be constructable externally to enforce clients go via ITextSanitisationHandler
    private SanitisedTextResult(SanitisedText text)
    {
        Value = text.Value;
    }

    internal static SanitisedTextResult Empty()
        => new(
            new SanitisedText(string.Empty));

    internal static SanitisedTextResult From(string value)
        => new(
            new SanitisedText(value));
}
