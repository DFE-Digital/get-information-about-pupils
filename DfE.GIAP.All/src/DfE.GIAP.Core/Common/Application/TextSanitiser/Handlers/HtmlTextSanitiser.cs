using DfE.GIAP.Core.Common.Application.TextSanitiser.Invoker;
using Ganss.Xss;

namespace DfE.GIAP.Core.Common.Application.TextSanitiser.Handlers;
internal sealed class HtmlTextSanitiser : ITextSanitiserHandler
{
    private static readonly HtmlSanitizer s_htmlSanitizer = new();

    static HtmlTextSanitiser()
    {
        s_htmlSanitizer.AllowedAttributes.Add("class");
    }
    public SanitisedText Handle(string raw)
    {
        string output = s_htmlSanitizer.Sanitize(raw);
        return new(output);
    }
}
