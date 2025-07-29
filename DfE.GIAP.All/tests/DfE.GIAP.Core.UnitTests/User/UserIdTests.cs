using DfE.GIAP.Core.User.Application;
using DfE.GIAP.SharedTests.TestDoubles.Users;

namespace DfE.GIAP.Core.UnitTests.User;
public sealed class UserIdTests
{
    [Fact]
    public void Constructor_WithValidId_SetsValueCorrectly()
    {
        // Arrange
        string input = "user-123";

        // Act
        UserId userId = new(input);

        // Assert
        Assert.Equal(input, userId.Value);
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
        UserId userId1 = UserIdTestDoubles.WithId(input);
        UserId userId2 = UserIdTestDoubles.WithId(input);

        // Act Assert
        Assert.Equal(userId1, userId2);
        Assert.True(userId1.Equals(userId2));
    }

    [Fact]
    public void Inequality_ShouldBeTrue_ForDifferentValues()
    {
        // Arrange
        UserId userId1 = UserIdTestDoubles.WithId("user-1");
        UserId userId2 = UserIdTestDoubles.WithId("user-2");

        // Act Assert
        Assert.NotEqual(userId1, userId2);
        Assert.False(userId1.Equals(userId2));
    }
}
