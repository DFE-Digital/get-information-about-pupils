using DfE.GIAP.Web.Shared.Serializer;
using Newtonsoft.Json;
using Xunit;

namespace DfE.GIAP.Web.Tests.Shared.Serializer;

public sealed class NewtonsoftJsonSerializerTests
{
    public record SerializeObjectMarker(int id, string name, Colour colour);
    public enum Colour { Red, Yellow, Green }

    // TODO complex objects e.g. collections, nested objects, enums, nullable types, date/time types, etc.
    // TODO casing of properties?
    // TODO should we be applying inputs read as JSON so that if we change Serialiser, either will conform to the same serialiser expectations?

    [Fact]
    public void Deserialize_Throws_When_Json_Is_Null()
    {
        // Arrange
        NewtonsoftJsonSerializer sut = new();

        // Act
        Action act = () => sut.Deserialize<SerializeObjectMarker>(null!);

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void Deserialize_Throws_When_Json_Is_Whitespace()
    {
        // Arrange
        NewtonsoftJsonSerializer sut = new();

        // Act
        Action act = () => sut.Deserialize<SerializeObjectMarker>("   ");

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void TryDeserialize_Throws_When_Json_Is_Null()
    {
        // Arrange
        NewtonsoftJsonSerializer sut = new();

        // Act
        Action act = () => sut.TryDeserialize<SerializeObjectMarker>(null!, out _);

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void TryDeserialize_Throws_When_Json_Is_Whitespace()
    {
        // Arrange
        NewtonsoftJsonSerializer sut = new();

        // Act
        Action act = () => sut.TryDeserialize<SerializeObjectMarker>("   ", out _);

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Deserialize_Returns_Object_When_Json_Is_Valid()
    {
        // Arrange
        NewtonsoftJsonSerializer sut = new();

        string json =
            """
            {
                "id": 1,
                "name": "test",
                "colour": 1
            }
            """;

        // Act
        SerializeObjectMarker result = sut.Deserialize<SerializeObjectMarker>(json);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.id);
        Assert.Equal("test", result.name);
        Assert.Equal(Colour.Yellow, result.colour);
    }

    [Fact]
    public void Deserialize_Throws_When_Json_Is_WellFormed_Null()
    {
        // Arrange
        NewtonsoftJsonSerializer sut = new();
        string json = "null";

        // Act
        Action act = () => sut.Deserialize<SerializeObjectMarker>(json);

        // Assert
        ArgumentException ex = Assert.Throws<ArgumentException>(act);
        Assert.Contains("Unable to deserialise", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Deserialize_Throws_JsonSerializationException_When_Json_Is_Malformed()
    {
        // Arrange
        NewtonsoftJsonSerializer sut = new();
        string json = "{ \"id\": 1, "; // malformed / truncated

        // Act
        Action act = () => sut.Deserialize<SerializeObjectMarker>(json);

        // Assert
        Assert.Throws<JsonSerializationException>(act);
    }

    [Fact]
    public void TryDeserialize_Returns_True_And_Sets_Value_When_Json_Is_Valid()
    {
        // Arrange
        NewtonsoftJsonSerializer sut = new();
        string json =
            """
            {
                "id": 7,
                "name": "ok",
                "colour": "Green"
            }
            """;

        // Act
        bool result = sut.TryDeserialize(json, out SerializeObjectMarker? value);

        // Assert
        Assert.True(result);
        Assert.NotNull(value);
        Assert.Equal(7, value!.id);
        Assert.Equal("ok", value.name);
        Assert.Equal(Colour.Green, value.colour);
    }

    [Fact]
    public void TryDeserialize_Returns_False_And_Sets_Value_Null_When_Json_Is_WellFormed_Null()
    {
        // Arrange
        NewtonsoftJsonSerializer sut = new();
        string json = "null";

        // Act
        bool result = sut.TryDeserialize(json, out SerializeObjectMarker? value);

        // Assert
        Assert.False(result);
        Assert.Null(value);
    }

    [Fact]
    public void TryDeserialize_Returns_False_And_Sets_Value_Null_When_Json_Is_Malformed()
    {
        // Arrange
        NewtonsoftJsonSerializer sut = new();
        string json = "{ \"id\": 1 "; // malformed / truncated

        // Act
        bool result = sut.TryDeserialize(json, out SerializeObjectMarker? value);

        // Assert
        Assert.False(result);
        Assert.Null(value);
    }

    [Fact]
    public void TryDeserialize_Does_Not_Throw_When_Json_Is_Malformed()
    {
        // Arrange
        NewtonsoftJsonSerializer sut = new();
        string json = "THIS_IS_NOT_JSON";

        // Act
        bool result = sut.TryDeserialize(json, out SerializeObjectMarker? value);

        // Assert
        Assert.False(result);
        Assert.Null(value);
    }

    [Fact]
    public void Serialize_Produces_NonEmpty_String_For_Object()
    {
        // Arrange
        NewtonsoftJsonSerializer sut = new();
        SerializeObjectMarker marker = new(7, "hey", Colour.Red);

        // Act
        string json = sut.Serialize(marker);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(json));
    }

    [Fact]
    public void Serialize_Then_TryDeserialize_RoundTrips_Data()
    {
        // Arrange
        NewtonsoftJsonSerializer sut = new();
        List<SerializeObjectMarker> markers = new()
        {
            new SerializeObjectMarker(1, "a", Colour.Yellow),
            new SerializeObjectMarker(2, "b", Colour.Green)
        };

        // Act
        string json = sut.Serialize(markers);
        bool ok = sut.TryDeserialize(json, out List<SerializeObjectMarker>? deserialized);

        // Assert
        Assert.True(ok);
        Assert.NotNull(deserialized);
        Assert.Equal(2, deserialized!.Count);

        Assert.Equal(1, deserialized[0].id);
        Assert.Equal("a", deserialized[0].name);
        Assert.Equal(Colour.Yellow, deserialized[0].colour);

        Assert.Equal(2, deserialized[1].id);
        Assert.Equal("b", deserialized[1].name);
        Assert.Equal(Colour.Green, deserialized[1].colour);
    }
}
