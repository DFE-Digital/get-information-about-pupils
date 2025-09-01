﻿using System.Globalization;
using DfE.GIAP.Core.SharedTests.TestDoubles;
using DfE.GIAP.Web.Session.Infrastructure.Serialization;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;
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

        DataTransferObjectStub expectedDto = new()
        {
            Id = expectedSessionObject.Id.ToString(),
            CreatedAt = expectedSessionObject.CreatedAt.ToString("O"),
            Metadata = new NestedTypeStub()
            {
                Source = expectedSessionObject.Source,
                Version = expectedSessionObject.Version
            }
        };


        MappedToDataTransferObjectSessionObjectSerializer<SessionObjectStub, DataTransferObjectStub> serializer = new(
                MapperTestDoubles.MockFor<SessionObjectStub, DataTransferObjectStub>(expectedDto).Object,
                MapperTestDoubles.MockFor<DataTransferObjectStub, SessionObjectStub>(expectedSessionObject).Object
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
        DataTransferObjectStub dto = new()
        {
            Id = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow.ToString("O"),
            Metadata = new NestedTypeStub()
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

        MappedToDataTransferObjectSessionObjectSerializer<SessionObjectStub, DataTransferObjectStub> serializer = new(
            MapperTestDoubles.MockFor<SessionObjectStub, DataTransferObjectStub>(dto).Object,
            MapperTestDoubles.MockFor<DataTransferObjectStub, SessionObjectStub>(expectedSessionObject).Object);

        // Act
        SessionObjectStub result = serializer.Deserialize(dtoAsJson);

        // Assert
        Assert.Equal(expectedSessionObject.Id, result.Id);
        Assert.Equal(expectedSessionObject.CreatedAt, result.CreatedAt);
        Assert.Equal(expectedSessionObject.Source, result.Source);
        Assert.Equal(expectedSessionObject.Version, result.Version);
    }

    public record SessionObjectStub(Guid Id, DateTime CreatedAt, string Source, int Version);

    public class NestedTypeStub
    {
        public string Source { get; set; }
        public int Version { get; set; }
    }

    public class DataTransferObjectStub
    {
        public string Id { get; set; }
        public string CreatedAt { get; set; }
        public NestedTypeStub Metadata { get; set; }
    }
}

