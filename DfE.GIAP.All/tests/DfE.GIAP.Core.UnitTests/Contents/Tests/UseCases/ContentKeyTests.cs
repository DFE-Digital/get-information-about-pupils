using DfE.GIAP.Core.Contents.Application.Models;

namespace DfE.GIAP.Core.UnitTests.Contents.Tests.UseCases;
public sealed class ContentKeyTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    public void ContentKey_ThrowsException_WhenConstructed_WithNullEmptyOrWhitespace(string? invalidId)
    {
        // Arrange
        Action construct = () => ContentKey.Create(invalidId!);

        // Act Assert
        Assert.Throws<ArgumentException>(construct);
    }

    [Fact]
    public void ContentKey_Trims_Whitespace_WhenConstructed()
    {
        // Arrange
        ContentKey key = ContentKey.Create("    \t  \n    test-key \r\n");

        // Act
        string result = key.ToString();

        // Assert
        Assert.Equal("test-key", result);
    }

    [Fact]
    public void ContentKey_ToString_Returns_Value()
    {
        // Arrange
        ContentKey key = ContentKey.Create("test-key");

        // Act
        string result = key.ToString();

        // Assert
        Assert.Equal("test-key", result);
    }

    [Fact]
    public void ContentKey_Equals_ReturnsTrue_When_ValuesAreEqual()
    {
        // Arrange
        ContentKey key1 = ContentKey.Create("test-key");
        ContentKey key2 = ContentKey.Create("test-key");

        // Act Assert
        Assert.True(key1.Equals(key2));
        Assert.True(key1.Equals((object)key2));
    }

    [Fact]
    public void ContentKey_Equals_ReturnsFalse_When_ValuesAreDifferent()
    {
        // Arrange
        ContentKey key1 = ContentKey.Create("test-key-a");
        ContentKey key2 = ContentKey.Create("test-key-b");

        // Act Assert
        Assert.False(key1.Equals(key2));
        Assert.False(key1.Equals((object)key2));
    }

    [Fact]
    public void ContentKey_Equals_ReturnsFalse_When_ComparedWithNull()
    {
        // Arrange
        ContentKey key = ContentKey.Create("test-key");

        // Act Assert
        Assert.False(key.Equals(null));
    }

    [Fact]
    public void ContentKey_GetHashCode_ReturnsSameValue_ForEqualKeys()
    {
        // Arrange
        ContentKey key1 = ContentKey.Create("test-key");
        ContentKey key2 = ContentKey.Create("test-key");

        // Act Assert
        Assert.Equal(key1.GetHashCode(), key2.GetHashCode());
    }

    [Fact]
    public void ContentKey_GetHashCode_ReturnsDifferentValue_ForDifferentKeys()
    {
        // Arrange
        ContentKey key1 = ContentKey.Create("test-key-a");
        ContentKey key2 = ContentKey.Create("test-key-b");

        // Act Assert
        Assert.NotEqual(key1.GetHashCode(), key2.GetHashCode());
    }
}
