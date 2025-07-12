using DfE.GIAP.Core.Common.Application.TextSanitiser.Sanitiser;

namespace DfE.GIAP.Core.Common.Application.TextSanitiser.Abstraction.Handler;

public interface ITextSanitiserHandler
{
    SanitisedTextResult Handle(string input);
}
