using DfE.GIAP.Web.Shared.Serializer;
using DfE.GIAP.Web.Shared.Session.Infrastructure.Serialization;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Shared.Session;

public sealed class DefaultSessionObjectSerializerTests
{
    [Fact]
    public void Constructor_Throws_When_JsonSerializer_Is_Null()
    {
        // Arrange
        Func<DefaultSessionObjectSerializer<SessionObjectStub>> act =
            () => new DefaultSessionObjectSerializer<SessionObjectStub>(null!);

        // Act Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void Serialize_Calls_JsonSerializer()
    {
        // Arrange
        const string serialisedResponse = "Test";

        Mock<IJsonSerializer> jsonSerializerMock = new();
        jsonSerializerMock.Setup(
            (serializer) => serializer.Serialize(
                It.IsAny<SessionObjectStub>()))
                    .Returns(serialisedResponse);

        DefaultSessionObjectSerializer<SessionObjectStub> serializer = new(jsonSerializerMock.Object);

        SessionObjectStub stub = SessionObjectStub.Create();

        // Act
        serializer.Serialize(stub);

        // Assert
        jsonSerializerMock.Verify(
            (serializer) => serializer.Serialize(
                It.Is<SessionObjectStub>((obj) => ReferenceEquals(stub, obj))), Times.Once);
    }

    [Fact]
    public void Deserialize_Calls_JsonSerializer()
    {
        // Arrange
        SessionObjectStub outputStub = SessionObjectStub.Create();

        Mock<IJsonSerializer> jsonSerializerMock = new();
        jsonSerializerMock.Setup(
            (serializer) =>
                serializer.Deserialize<SessionObjectStub>(It.IsAny<string>()))
                    .Returns(outputStub);

        DefaultSessionObjectSerializer<SessionObjectStub> serializer = new(jsonSerializerMock.Object);

        const string inputJson = "{}";

        // Act
        SessionObjectStub response = serializer.Deserialize(inputJson);

        // Assert
        Assert.Same(outputStub, response);

        jsonSerializerMock.Verify(
            (serializer) => serializer.Deserialize<SessionObjectStub>(
                It.Is<string>((obj) => ReferenceEquals(inputJson, obj))), Times.Once);
    }

    private class SessionObjectStub
    {
        public int Id { get; set; }
        public string? Name { get; set; }

        public static SessionObjectStub Create() => new()
        {
            Id = 1,
            Name = "Test"
        };
    }
}
