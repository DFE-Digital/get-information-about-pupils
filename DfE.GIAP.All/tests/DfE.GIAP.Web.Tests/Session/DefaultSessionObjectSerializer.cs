using DfE.GIAP.Web.Shared.Session.Infrastructure.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Session;

public sealed class DefaultSessionObjectSerializerTests
{
    private readonly DefaultSessionObjectSerializer<TestSessionObject> _serializer = new();

    [Fact]
    public void Serialize_ShouldReturnJsonString()
    {
        // Arrange
        TestSessionObject sessionObject = new()
        {
            Id = 1,
            Name = "Test"
        };

        // Act
        string result = _serializer.Serialize(sessionObject);

        // Assert
        JObject response = JObject.Parse(result);
        Assert.Equal(2, response.Children().Count());
        Assert.Equal(1, (int)response["Id"]!);
        Assert.Equal("Test", response["Name"]!.ToString());
    }

    [Fact]
    public void Deserialize_MultiLineJson_ShouldWork()
    {
        // Arrange
        string json = @"
        {
            ""Id"": 1,
            ""Name"": ""Test""
        }";

        // Act
        TestSessionObject result = _serializer.Deserialize(json);

        // Assert
        Assert.Equal(1, result.Id);
        Assert.Equal("Test", result.Name);
    }


    [Fact]
    public void Deserialize_InvalidJson_ShouldThrow()
    {
        // Arrange
        string invalidJson = "{invalid json}";

        // Act & Assert
        Assert.ThrowsAny<JsonException>(() => _serializer.Deserialize(invalidJson));
    }

    private class TestSessionObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
