namespace DfE.GIAP.Core.Common.Application.TextSanitiser.Invoker;

public readonly struct SanitisedText
{
    public string Value { get; }

    public SanitisedText(string? value)
    {
        Value = value ?? string.Empty;
    }
}
