﻿using DfE.GIAP.Core.Common.Application.TextSanitiser.Invoker;

namespace DfE.GIAP.Core.Common.Application.TextSanitiser.Handlers;
public sealed class SanitisedTextResult
{
    public string Value { get; }
    // NOT to be constructable external to the assembly
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
