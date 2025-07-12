using DfE.GIAP.Core.Common.Application.TextSanitiser.Handlers;

namespace DfE.GIAP.Core.Common.Application.TextSanitiser.Invoker;
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
