using DfE.GIAP.Core.Common.Application.TextSanitiser.Handlers;
using DfE.GIAP.Core.Common.Application.TextSanitiser.Invoker;
using Ganss.Xss;

namespace DfE.GIAP.Web.Helpers.TextSanitiser;

internal sealed class HtmlTextSanitiser : ITextSanitiserHandler
{
    private static readonly HtmlSanitizer s_htmlSanitizer = new();

    static HtmlTextSanitiser()
    {
        s_htmlSanitizer.AllowedAttributes.Add("class");
    }
    public SanitisedText Handle(string raw)
    {
        if (string.IsNullOrEmpty(raw))
        {
            return new(string.Empty);
        }

        string output = s_htmlSanitizer.Sanitize(raw);
        return new(output);
    }
}
