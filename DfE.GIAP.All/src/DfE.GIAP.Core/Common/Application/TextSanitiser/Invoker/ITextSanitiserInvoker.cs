using DfE.GIAP.Core.Common.Application.TextSanitiser.Handlers;

namespace DfE.GIAP.Core.Common.Application.TextSanitiser.Invoker;

public interface ITextSanitiserInvoker
{
    SanitisedTextResult Sanitise(string input);
}
