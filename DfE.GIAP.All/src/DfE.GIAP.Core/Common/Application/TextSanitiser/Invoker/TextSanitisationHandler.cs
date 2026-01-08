using DfE.GIAP.Core.Common.Application.TextSanitiser.Handlers;

namespace DfE.GIAP.Core.Common.Application.TextSanitiser.Invoker;
internal sealed class TextSanitiser : ITextSanitiser
{
    private readonly List<ITextSanitiserHandler> _sanitisers;

    public TextSanitiser(IEnumerable<ITextSanitiserHandler> sanitisers)
    {
        _sanitisers = [];
        _sanitisers.AddRange(sanitisers ?? []);
    }

    public SanitisedTextResult Sanitise(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return SanitisedTextResult.Empty();
        }

        string current = input;
        foreach (ITextSanitiserHandler sanitiser in _sanitisers)
        {
            SanitisedText result = sanitiser.Handle(current);
            current = result.Value;
        }

        return SanitisedTextResult.From(current);
    }
}
