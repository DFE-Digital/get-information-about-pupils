using DfE.GIAP.Web.Shared.Session.Abstraction.Command;
using Moq;

namespace DfE.GIAP.Web.Tests.Shared.Session.TestDoubles;
internal static class ISessionCommandHandlerTestDoubles
{
    internal static Mock<ISessionCommandHandler<TSessionObject>> Default<TSessionObject>() where TSessionObject : class
        => new();
}
