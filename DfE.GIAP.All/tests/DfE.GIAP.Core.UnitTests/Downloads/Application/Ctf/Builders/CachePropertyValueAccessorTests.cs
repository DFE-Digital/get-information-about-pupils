using DfE.GIAP.Core.Downloads.Application.Ctf.Builders;

namespace DfE.GIAP.Core.UnitTests.Downloads.Application.Ctf.Builders;

public class CachePropertyValueAccessorTests
{
    private class TestObject
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string? NullableField { get; set; }
    }

    [Fact]
    public void GetValue_ReturnsNull_WhenInstanceIsNull()
    {
        // Arrange
        CachePropertyValueAccessor accessor = new CachePropertyValueAccessor();

        // Act
        string? result = accessor.GetValue(null!, "Name");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetValue_ReturnsNull_WhenFieldNameIsNull()
    {
        // Arrange
        CachePropertyValueAccessor accessor = new CachePropertyValueAccessor();
        TestObject instance = new TestObject();

        // Act
        string? result = accessor.GetValue(instance, null!);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetValue_ReturnsNull_WhenFieldDoesNotExist()
    {
        // Arrange
        CachePropertyValueAccessor accessor = new CachePropertyValueAccessor();
        TestObject instance = new TestObject();

        // Act
        string? result = accessor.GetValue(instance, "DoesNotExist");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetValue_ReturnsStringPropertyValue()
    {
        // Arrange
        CachePropertyValueAccessor accessor = new CachePropertyValueAccessor();
        TestObject instance = new TestObject { Name = "Alice" };

        // Act
        string? result = accessor.GetValue(instance, "Name");

        // Assert
        Assert.Equal("Alice", result);
    }

    [Fact]
    public void GetValue_ReturnsConvertedValue_ForNonStringProperty()
    {
        // Arrange
        CachePropertyValueAccessor accessor = new CachePropertyValueAccessor();
        TestObject instance = new TestObject { Age = 42 };

        // Act
        string? result = accessor.GetValue(instance, "Age");

        // Assert
        Assert.Equal("42", result);
    }

    [Fact]
    public void GetValue_IsCaseInsensitive()
    {
        // Arrange
        CachePropertyValueAccessor accessor = new CachePropertyValueAccessor();
        TestObject instance = new TestObject { Name = "Bob" };

        // Act
        string? result = accessor.GetValue(instance, "nAmE");

        // Assert
        Assert.Equal("Bob", result);
    }

    [Fact]
    public void GetValue_CachesAccessor_AfterFirstCall()
    {
        // Arrange
        CachePropertyValueAccessor accessor = new CachePropertyValueAccessor();
        TestObject instance = new TestObject { Name = "Charlie" };

        // First call populates cache
        string? first = accessor.GetValue(instance, "Name");

        // Modify the property to ensure accessor is reused
        instance.Name = "Updated";

        // Act
        string? second = accessor.GetValue(instance, "Name");

        // Assert
        Assert.Equal("Updated", second);
    }

    [Fact]
    public void GetValue_ReturnsNull_WhenNullablePropertyIsNull()
    {
        // Arrange
        CachePropertyValueAccessor accessor = new CachePropertyValueAccessor();
        TestObject instance = new TestObject { NullableField = null };

        // Act
        string? result = accessor.GetValue(instance, "NullableField");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetValue_ReturnsValue_WhenNullablePropertyHasValue()
    {
        // Arrange
        CachePropertyValueAccessor accessor = new CachePropertyValueAccessor();
        TestObject instance = new TestObject { NullableField = "Hello" };

        // Act
        string? result = accessor.GetValue(instance, "NullableField");

        // Assert
        Assert.Equal("Hello", result);
    }
}
