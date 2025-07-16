using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.UnitTests.MyPupils.Domain.ValueObjects;
public class UniquePupilNumberTests
{
    [Fact]
    public void Constructor_WithValidValue_SetsValueCorrectly()
    {
        // Arrange
        string input = "A12345678901";

        // Act
        UniquePupilNumber upn = new(input);

        // Assert
        Assert.Equal("A12345678901", upn.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\r\n")]
    [InlineData("   \r\n ")]
    [InlineData("\n")]
    public void TryCreate_WithNullOrWhitespace_ReturnsFalse(string? input)
    {
        // Arrange
        UniquePupilNumber? result;

        // Act
        bool success = UniquePupilNumber.TryCreate(input, out result);

        // Assert
        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryCreate_WithValidInput_ReturnsTrueAndSetsResult()
    {
        // Arrange
        string input = "B98765432109";

        // Act
        bool success = UniquePupilNumber.TryCreate(input, out UniquePupilNumber? result);

        // Assert
        Assert.True(success);
        Assert.NotNull(result);
        Assert.Equal("B98765432109", result!.Value);
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        // Arrange
        string input = "C11223344556";
        UniquePupilNumber upn = new(input);

        // Act
        string result = upn.ToString();

        // Assert
        Assert.Equal(input, result);
    }

    [Fact]
    public void Equality_ShouldBeBasedOnValue()
    {
        // Arrange
        UniquePupilNumber upn1 = new("D12345678901");
        UniquePupilNumber upn2 = new("D12345678901");

        // Act Assert
        Assert.Equal(upn1, upn2);
        Assert.True(upn1.Equals(upn2));
    }

    [Fact]
    public void Inequality_ShouldBeTrue_ForDifferentValues()
    {
        // Arrange
        UniquePupilNumber upn1 = new("E12345678901");
        UniquePupilNumber upn2 = new("F98765432109");

        // Act Assert
        Assert.NotEqual(upn1, upn2);
        Assert.False(upn1.Equals(upn2));
    }
}
