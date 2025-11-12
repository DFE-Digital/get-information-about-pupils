using DfE.GIAP.Web.Session.Abstraction;
using Moq;

namespace DfE.GIAP.Web.Tests.Session.TestDoubles;
internal static class ISessionObjectSerializerTestDoubles
{
    internal static Mock<ISessionObjectSerializer<TSessionObject>> Default<TSessionObject>() => new();

    internal static Mock<ISessionObjectSerializer<TSessionObject>> MockDeserialize<TSessionObject>(TSessionObject stubSessionObject)
    {
        Mock<ISessionObjectSerializer<TSessionObject>> mockSerializer = Default<TSessionObject>();

        mockSerializer
            .Setup((serializer) => serializer.Deserialize(It.IsAny<string>()))
            .Returns(stubSessionObject)
            .Verifiable();

        return mockSerializer;
    }

    internal static Mock<ISessionObjectSerializer<TSessionObject>> MockSerialize<TSessionObject>(string stubSerialisedValue)
    {
        Mock<ISessionObjectSerializer<TSessionObject>> mockSerializer = Default<TSessionObject>();

        mockSerializer
            .Setup((serializer) => serializer.Serialize(It.IsAny<TSessionObject>()))
            .Returns(stubSerialisedValue)
            .Verifiable();

        return mockSerializer;
    }
}
