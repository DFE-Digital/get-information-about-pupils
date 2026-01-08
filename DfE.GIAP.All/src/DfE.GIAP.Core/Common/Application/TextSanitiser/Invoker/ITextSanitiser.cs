using DfE.GIAP.Core.Common.Application.TextSanitiser.Handlers;

namespace DfE.GIAP.Core.Common.Application.TextSanitiser.Invoker;

public interface ITextSanitiser
{
    SanitisedTextResult Sanitise(string input);
}
