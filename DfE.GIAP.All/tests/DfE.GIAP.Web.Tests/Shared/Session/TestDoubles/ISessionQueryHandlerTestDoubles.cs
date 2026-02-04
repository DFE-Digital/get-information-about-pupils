using DfE.GIAP.Web.Shared.Session.Abstraction;
using DfE.GIAP.Web.Shared.Session.Abstraction.Query;

namespace DfE.GIAP.Web.Tests.Shared.Session.TestDoubles;

internal static class ISessionQueryHandlerTestDoubles
{
    internal static Mock<ISessionQueryHandler<TSessionObject>> Default<TSessionObject>() where TSessionObject : class
        => new();

    internal static Mock<ISessionQueryHandler<TSessionObject>> MockFor<TSessionObject>(TSessionObject stubSessionObject) where TSessionObject : class
    {
        Mock<ISessionQueryHandler<TSessionObject>> mock = Default<TSessionObject>();
        mock.Setup(t => t.Handle(It.IsAny<SessionCacheKey>()))
            .Returns(SessionQueryResponse<TSessionObject>.Create(stubSessionObject))
            .Verifiable();

        return mock;
    }

    internal static Mock<ISessionQueryHandler<TSessionObject>> MockFor<TSessionObject>(SessionQueryResponse<TSessionObject> response) where TSessionObject : class
    {
        Mock<ISessionQueryHandler<TSessionObject>> mock = Default<TSessionObject>();
        mock.Setup(t => t.Handle(It.IsAny<SessionCacheKey>()))
            .Returns(response)
            .Verifiable();

        return mock;
    }
}
