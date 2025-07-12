using DfE.GIAP.Core.Common.Application.TextSanitiser.Invoker;

namespace DfE.GIAP.Core.Common.Application.TextSanitiser.Handlers;

public interface ITextSanitiserHandler
{
    SanitisedText Handle(string raw);
}
