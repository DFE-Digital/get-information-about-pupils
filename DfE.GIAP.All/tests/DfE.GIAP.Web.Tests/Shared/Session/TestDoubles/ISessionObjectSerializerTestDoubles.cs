using DfE.GIAP.Web.Shared.Session.Abstraction;
using Moq;

namespace DfE.GIAP.Web.Tests.Shared.Session.TestDoubles;
internal static class ISessionObjectSerializerTestDoubles
{
    internal static Mock<ISessionObjectSerializer<TSessionObject>> Default<TSessionObject>() where TSessionObject : class => new();

    internal static Mock<ISessionObjectSerializer<TSessionObject>> MockDeserialize<TSessionObject>(TSessionObject stubSessionObject) where TSessionObject : class
    {
        Mock<ISessionObjectSerializer<TSessionObject>> mockSerializer = Default<TSessionObject>();

        mockSerializer
            .Setup((serializer) => serializer.Deserialize(It.IsAny<string>()))
            .Returns(stubSessionObject)
            .Verifiable();

        return mockSerializer;
    }

    internal static Mock<ISessionObjectSerializer<TSessionObject>> MockSerialize<TSessionObject>(string stubSerialisedValue) where TSessionObject : class
    {
        Mock<ISessionObjectSerializer<TSessionObject>> mockSerializer = Default<TSessionObject>();

        mockSerializer
            .Setup((serializer) => serializer.Serialize(It.IsAny<TSessionObject>()))
            .Returns(stubSerialisedValue)
            .Verifiable();

        return mockSerializer;
    }
}
