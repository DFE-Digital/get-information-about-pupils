using DfE.GIAP.Web.Features.MyPupils.GetMyPupilsForUser.UpdateCurrentPageOfPupilsHandler;
using DfE.GIAP.Web.Features.MyPupils.State.Selection;
using DfE.GIAP.Web.Session.Abstraction.Command;
using DfE.GIAP.Web.Session.Abstraction.Query;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.GetMyPupils;
public sealed class UpdateCurrentPageOfPupilsStateHandlerTests
{
    [Fact]
    public void Constructor_Throws_When_QueryHandler_Is_Null()
    {
        Mock<ISessionCommandHandler<MyPupilsPupilSelectionState>> commandHandler = ISessionCommandHandlerTestDoubles.Default<MyPupilsPupilSelectionState>();
        Func<UpdateCurrentPageOfPupilsHandler> construct = () => new(null, commandHandler.Object);

        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_CommandHandler_Is_Null()
    {
        Mock<ISessionQueryHandler<MyPupilsPupilSelectionState>> queryHandler = ISessionQueryHandlerTestDoubles.Default<MyPupilsPupilSelectionState>();
        Func<UpdateCurrentPageOfPupilsHandler> construct = () => new(queryHandler.Object, null);

        Assert.Throws<ArgumentNullException>(construct);
    }
}


internal static class ISessionCommandHandlerTestDoubles
{
    internal static Mock<ISessionCommandHandler<TSessionObject>> Default<TSessionObject>() where TSessionObject : class
        => new();
}

internal static class ISessionQueryHandlerTestDoubles
{
    internal static Mock<ISessionQueryHandler<TSessionObject>> Default<TSessionObject>() where TSessionObject : class
        => new();

    internal static Mock<ISessionQueryHandler<TSessionObject>> MockFor<TSessionObject>(TSessionObject stubSessionObject) where TSessionObject : class
    {
        Mock<ISessionQueryHandler<TSessionObject>> mock = Default<TSessionObject>();
        mock.Setup(t => t.GetSessionObject())
            .Returns(SessionQueryResponse<TSessionObject>.Create(stubSessionObject))
            .Verifiable();

        return mock;
    }
}
