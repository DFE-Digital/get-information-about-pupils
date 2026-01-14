using System.Text;
using Microsoft.AspNetCore.Http;
using Moq;

namespace DfE.GIAP.Web.Tests.Shared.Session.TestDoubles;
internal interface ISessionTestDoubles
{
    internal static Mock<ISession> Default() => new();

    internal static Mock<ISession> MockForTryGetValue(string key, bool tryGetResult)
    {
        Mock<ISession> mockSession = Default();

        // Must match 
        byte[] stubBytes = Encoding.UTF8.GetBytes(key);
        mockSession
            .Setup(session => session.TryGetValue(It.IsAny<string>(), out stubBytes!))
            .Returns(tryGetResult);

        return mockSession;
    }

    internal static Mock<ISession> MockForSet()
    {
        Mock<ISession> mockSession = Default();

        mockSession
            .Setup((session) => session.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
            .Verifiable();

        return mockSession;
    }
}
