using DfE.GIAP.Core.Common.Application.TextSanitiser.Sanitiser;

namespace DfE.GIAP.Core.Common.Application.TextSanitiser.Abstraction.Handler;

public interface ITextSanitiserInvoker
{
    SanitisedTextResult Handle(string input);
}
