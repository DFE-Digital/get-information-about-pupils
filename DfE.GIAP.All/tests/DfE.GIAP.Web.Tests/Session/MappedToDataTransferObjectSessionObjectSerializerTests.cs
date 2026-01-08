using System.Globalization;
using DfE.GIAP.SharedTests.Common;
using DfE.GIAP.Web.Shared.Session.Infrastructure.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Session;

public class MappedToDataTransferObjectSessionObjectSerializerTests
{
    [Fact]
    public void Serialize_ShouldMapComplexObjectToDtoAndSerialize()
    {
        // Arrange
        SessionObjectStub expectedSessionObject = new(
            Id: Guid.NewGuid(),
            CreatedAt: DateTime.UtcNow,
            Source: "System",
            Version: 2);

        DtoStub expectedDto = new()
        {
            Id = expectedSessionObject.Id.ToString(),
            CreatedAt = expectedSessionObject.CreatedAt.ToString("O"),
            Metadata = new NestedDtoStub()
            {
                Source = expectedSessionObject.Source,
                Version = expectedSessionObject.Version
            }
        };


        MappedToDataTransferObjectSessionObjectSerializer<SessionObjectStub, DtoStub> serializer = new(
                MapperTestDoubles.MockFor<SessionObjectStub, DtoStub>(expectedDto).Object,
                MapperTestDoubles.Default<DtoStub, SessionObjectStub>().Object
            );

        // Act
        string json = serializer.Serialize(expectedSessionObject);

        // Assert
        JObject response = JObject.Parse(json);
        Assert.Equal(3, response.Children().Count());
        Assert.Equal(expectedDto.Id, response["Id"]!);
        Assert.Equal(expectedDto.CreatedAt, response["CreatedAt"]!);

        Assert.Equal(2, response["Metadata"].Children().Count());
        Assert.Equal("System", response["Metadata"]!["Source"]!.ToString());
        Assert.Equal(2, (int)response["Metadata"]!["Version"]);
    }

    [Fact]
    public void Deserialize_ShouldMapDtoBackToComplexObject()
    {
        // Arrange
        DtoStub dto = new()
        {
            Id = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow.ToString("O"),
            Metadata = new NestedDtoStub()
            {
                Source = "System",
                Version = 2
            }
        };

        string dtoAsJson = JsonConvert.SerializeObject(dto);

        SessionObjectStub expectedSessionObject = new(
            Id: Guid.Parse(dto.Id),
            CreatedAt: DateTime.Parse(dto.CreatedAt, CultureInfo.CurrentCulture),
            Source: dto.Metadata.Source,
            Version: dto.Metadata.Version);

        MappedToDataTransferObjectSessionObjectSerializer<SessionObjectStub, DtoStub> serializer = new(
            MapperTestDoubles.Default<SessionObjectStub, DtoStub>().Object,
            MapperTestDoubles.MockFor<DtoStub, SessionObjectStub>(expectedSessionObject).Object);

        // Act
        SessionObjectStub actualResult = serializer.Deserialize(dtoAsJson);

        // Assert
        Assert.Equal(expectedSessionObject, actualResult);
    }

    public record SessionObjectStub(Guid Id, DateTime CreatedAt, string Source, int Version);

    public sealed class DtoStub
    {
        public string Id { get; set; }
        public string CreatedAt { get; set; }
        public NestedDtoStub Metadata { get; set; }
    }

    public sealed class NestedDtoStub
    {
        public string Source { get; set; }
        public int Version { get; set; }
    }
}

