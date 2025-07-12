using DfE.GIAP.Core.Common.Application.TextSanitiser.Abstraction.Handler;
using DfE.GIAP.Core.Common.Application.TextSanitiser.Sanitiser;

namespace DfE.GIAP.Core.Common.Application.TextSanitiser.Handler;
internal sealed class TextSanitiserHandler : ITextSanitiserHandler
{
    private readonly List<ITextSanitiser> _sanitisers;

    public TextSanitiserHandler(IEnumerable<ITextSanitiser> sanitisers)
    {
        _sanitisers = [];
        _sanitisers.AddRange(sanitisers ?? []);
        _sanitisers.Add(new HtmlTextSanitiser()); // ensure this runs last in case the custom sanitisers are malicious or misconfigured
    }

    public SanitisedTextResult Handle(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return SanitisedTextResult.Empty();
        }

        string current = input;
        foreach (ITextSanitiser sanitiser in _sanitisers)
        {
            SanitisedText result = sanitiser.Sanitise(current);
            current = result.Value;
        }

        return SanitisedTextResult.From(current);
    }
}
