using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.UnitTests.MyPupils.Domain.ValueObjects;
public class UserIdTests
{
    [Fact]
    public void Constructor_WithValidId_SetsValueCorrectly()
    {
        // Arrange
        string input = "user-123";

        // Act
        UserId userId = new(input);

        // Assert
        Assert.Equal("user-123", userId.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\r\n")]
    [InlineData("   \r\n ")]
    [InlineData("\n")]
    public void Constructor_WithNullOrWhitespace_ThrowsArgumentException(string? input)
    {
        // Arrange Act Assert
        Assert.ThrowsAny<ArgumentException>(() => new UserId(input!));
    }

    [Fact]
    public void Equality_ShouldBeBasedOnValue()
    {
        // Arrange
        const string input = "abc-123";
        UserId userId1 = new(input);
        UserId userId2 = new(input);

        // Act Assert
        Assert.Equal(userId1, userId2);
        Assert.True(userId1.Equals(userId2));
    }

    [Fact]
    public void Inequality_ShouldBeTrue_ForDifferentValues()
    {
        // Arrange
        UserId userId1 = new("user-1");
        UserId userId2 = new("user-2");

        // Act Assert
        Assert.NotEqual(userId1, userId2);
        Assert.False(userId1.Equals(userId2));
    }
}
