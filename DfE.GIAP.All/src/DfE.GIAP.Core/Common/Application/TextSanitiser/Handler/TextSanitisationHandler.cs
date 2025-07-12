using DfE.GIAP.Core.Common.Application.TextSanitiser.Abstraction.Handler;
using DfE.GIAP.Core.Common.Application.TextSanitiser.Sanitiser;

namespace DfE.GIAP.Core.Common.Application.TextSanitiser.Handler;
internal sealed class TextSanitisationInvoker : ITextSanitiserInvoker
{
    private readonly List<ITextSanitiserHandler> _sanitisers;

    public TextSanitisationInvoker(IEnumerable<ITextSanitiserHandler> sanitisers)
    {
        _sanitisers = [];
        _sanitisers.AddRange(sanitisers ?? []);
        _sanitisers.Add(new HtmlTextSanitiser()); // ensure this runs last in case the custom sanitisers are malicious or misconfigured
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
