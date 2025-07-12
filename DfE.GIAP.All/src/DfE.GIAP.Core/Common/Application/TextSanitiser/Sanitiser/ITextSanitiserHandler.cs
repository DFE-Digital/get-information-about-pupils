using DfE.GIAP.Core.Common.Application.TextSanitiser.Abstraction.Handler;

namespace DfE.GIAP.Core.Common.Application.TextSanitiser.Sanitiser;

public interface ITextSanitiserHandler
{
    SanitisedText Sanitise(string raw);
}
