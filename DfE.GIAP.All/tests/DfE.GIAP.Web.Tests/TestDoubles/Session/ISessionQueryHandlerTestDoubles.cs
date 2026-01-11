using DfE.GIAP.Web.Shared.Session.Abstraction.Query;
using Moq;

namespace DfE.GIAP.Web.Tests.TestDoubles.Session;

internal static class ISessionQueryHandlerTestDoubles
{
    internal static Mock<ISessionQueryHandler<TSessionObject>> Default<TSessionObject>() where TSessionObject : class
        => new();

    internal static Mock<ISessionQueryHandler<TSessionObject>> MockFor<TSessionObject>(TSessionObject stubSessionObject) where TSessionObject : class
    {
        Mock<ISessionQueryHandler<TSessionObject>> mock = Default<TSessionObject>();
        mock.Setup(t => t.Handle())
            .Returns(SessionQueryResponse<TSessionObject>.Create(stubSessionObject))
            .Verifiable();

        return mock;
    }

    internal static Mock<ISessionQueryHandler<TSessionObject>> MockFor<TSessionObject>(SessionQueryResponse<TSessionObject> response) where TSessionObject : class
    {
        Mock<ISessionQueryHandler<TSessionObject>> mock = Default<TSessionObject>();
        mock.Setup(t => t.Handle())
            .Returns(response)
            .Verifiable();

        return mock;
    }
}
