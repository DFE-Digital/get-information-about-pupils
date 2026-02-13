using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.SharedTests.Common;
using DfE.GIAP.Web.Shared.Serializer;
using DfE.GIAP.Web.Shared.Session.Infrastructure.Serialization;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Shared.Session;
public sealed class MappedToDataTransferObjectSessionObjectSerializerTests
{
    [Fact]
    public void Constructor_Throws_When_ToDtoMapper_Is_Null()
    {
        // Arrange
        Func<MappedToDataTransferObjectSessionObjectSerializer<SessionObjectStub, DtoStub>> construct =
            () => new MappedToDataTransferObjectSessionObjectSerializer<SessionObjectStub, DtoStub>(
                null!,
                MapperTestDoubles.Default<DtoStub, SessionObjectStub>().Object,
                new Mock<IJsonSerializer>().Object);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_FromDtoMapper_Is_Null()
    {
        // Arrange
        Func<MappedToDataTransferObjectSessionObjectSerializer<SessionObjectStub, DtoStub>> construct =
            () => new MappedToDataTransferObjectSessionObjectSerializer<SessionObjectStub, DtoStub>(
                MapperTestDoubles.Default<SessionObjectStub, DtoStub>().Object,
                null!,
                new Mock<IJsonSerializer>().Object);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_JsonSerializer_Is_Null()
    {
        // Arrange
        Func<MappedToDataTransferObjectSessionObjectSerializer<SessionObjectStub, DtoStub>> construct =
            () => new MappedToDataTransferObjectSessionObjectSerializer<SessionObjectStub, DtoStub>(
                MapperTestDoubles.Default<SessionObjectStub, DtoStub>().Object,
                MapperTestDoubles.Default<DtoStub, SessionObjectStub>().Object,
                null!);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Serialize_Maps_To_Dto_And_Calls_JsonSerializer()
    {
        // Arrange
        SessionObjectStub expectedSessionObject = SessionObjectStub.CreateDefault();

        DtoStub expectedDto = DtoStub.CreateDefault();

        Mock<IMapper<SessionObjectStub, DtoStub>> toDtoMapperMock =
            MapperTestDoubles.MockFor<SessionObjectStub, DtoStub>(expectedDto);

        const string serialisedJson = "{\"the\":\"json\"}";

        Mock<IJsonSerializer> jsonSerializerMock = new();
        jsonSerializerMock
            .Setup(s => s.Serialize(It.IsAny<object>()))
            .Returns(serialisedJson);

        MappedToDataTransferObjectSessionObjectSerializer<SessionObjectStub, DtoStub> serializer =
            new(toDtoMapperMock.Object,
                MapperTestDoubles.Default<DtoStub, SessionObjectStub>().Object,
                jsonSerializerMock.Object);

        // Act
        string json = serializer.Serialize(expectedSessionObject);

        // Assert
        Assert.Same(serialisedJson, json);

        jsonSerializerMock.Verify(
            (serializer)
                => serializer.Serialize(
                    It.Is<object>((obj) => ReferenceEquals(obj, expectedDto))),
            Times.Once);

        toDtoMapperMock.Verify(
            (mapper) =>
                mapper.Map(
                    It.Is<SessionObjectStub>((obj) => ReferenceEquals(obj, expectedSessionObject))),
            Times.Once);
    }

    [Fact]
    public void Deserialize_Calls_JsonSerializer_And_Maps_Back_To_SessionObject()
    {
        // Arrange
        DtoStub dto = DtoStub.CreateDefault();

        SessionObjectStub expectedSessionObject = SessionObjectStub.CreateDefault();

        Mock<IMapper<SessionObjectStub, DtoStub>> toDtoMapperMock =
            MapperTestDoubles.Default<SessionObjectStub, DtoStub>();

        Mock<IMapper<DtoStub, SessionObjectStub>> fromDtoMapperMock =
            MapperTestDoubles.MockFor<DtoStub, SessionObjectStub>(expectedSessionObject);

        const string inputJson = "{}";

        Mock<IJsonSerializer> jsonSerializerMock = new Mock<IJsonSerializer>();
        jsonSerializerMock
            .Setup(s => s.Deserialize<DtoStub>(It.IsAny<string>()))
            .Returns(dto);

        MappedToDataTransferObjectSessionObjectSerializer<SessionObjectStub, DtoStub> serializer =
            new MappedToDataTransferObjectSessionObjectSerializer<SessionObjectStub, DtoStub>(
                toDtoMapperMock.Object,
                fromDtoMapperMock.Object,
                jsonSerializerMock.Object);

        // Act
        SessionObjectStub actual = serializer.Deserialize(inputJson);

        // Assert
        Assert.Equal(expectedSessionObject, actual);

        jsonSerializerMock.Verify(
            s => s.Deserialize<DtoStub>(
                It.Is<string>(str => ReferenceEquals(str, inputJson))),
            Times.Once);

        fromDtoMapperMock.Verify(
            m => m.Map(It.Is<DtoStub>(d => ReferenceEquals(d, dto))),
            Times.Once);
    }
    public record SessionObjectStub(Guid Id, DateTime CreatedAt, string Source, int Version)
    {
        public static SessionObjectStub CreateDefault() =>
            new(
                Id: Guid.NewGuid(),
                CreatedAt: DateTime.UtcNow,
                Source: "System",
                Version: 2);
    }

    public sealed class DtoStub
    {
        public string? Id { get; set; }
        public string? CreatedAt { get; set; }
        public NestedDtoStub? Metadata { get; set; }

        public static DtoStub CreateDefault() => new()
        {
            Id = "Test Dto",
            CreatedAt = DateTime.UtcNow.ToString(),
            Metadata = new()
            {
                Source = "Source",
                Version = 100
            }
        };

        public sealed class NestedDtoStub
        {
            public string? Source { get; set; }
            public int? Version { get; set; }
        }
    }

}
