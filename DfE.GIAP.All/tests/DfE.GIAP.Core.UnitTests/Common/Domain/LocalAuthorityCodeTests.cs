using DfE.GIAP.Core.Common.Domain;

namespace DfE.GIAP.Core.UnitTests.Common.Domain;
public class LocalAuthorityCodeTests
{
    [Fact]
    public void Constructor_SetsCode_WhenValid()
    {
        // Arrange
        int validCode = 999;

        // Act
        LocalAuthorityCode result = new(validCode);

        // Assert
        Assert.Equal(validCode, result.Code);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Constructor_ThrowsArgumentException_WhenCodeIsNegative(int invalidCode)
    {
        // Act & Assert
        ArgumentException ex = Assert.Throws<ArgumentOutOfRangeException>(() => new LocalAuthorityCode(invalidCode));
    }

    [Theory]
    [InlineData(1000)]
    [InlineData(1234)]
    public void Constructor_ThrowsArgumentOutOfRangeException_WhenCodeIsTooLarge(int invalidCode)
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new LocalAuthorityCode(invalidCode));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("\r\n")]
    [InlineData("   \t   ")]
    public void Constructor_ThrowsArgumentException_WhenCodeIsNullOrWhiteSpace(string? code)
    {
        // Act & Assert
        ArgumentException exception = Assert.ThrowsAny<ArgumentException>(() => new LocalAuthorityCode(code!));
    }

    [Theory]
    [InlineData("ABC")]
    [InlineData("12A")]
    [InlineData("A12")]
    [InlineData("1_2")]
    [InlineData("9-9")]
    public void Constructor_ThrowsArgumentException_WhenCodeIsNotNumeric(string code)
    {
        // Act
        ArgumentException exception = Assert.Throws<ArgumentException>(() => new LocalAuthorityCode(code));
    }

    [Theory]
    [InlineData("0", 0)]
    [InlineData("1", 1)]
    [InlineData("9", 9)]
    [InlineData("10", 10)]
    [InlineData("999", 999)]
    [InlineData("007", 7)]
    public void Constructor_SetsCode_WhenValidNumericString(string code, int expected)
    {
        // Act
        LocalAuthorityCode result = new(code);

        // Assert
        Assert.Equal(expected, result.Code);
    }

    [Theory]
    [InlineData(" 1 ", 1)]
    [InlineData("  0", 0)]
    [InlineData("999  ", 999)]
    [InlineData("\t007\r\n", 7)]
    public void Constructor_SetsCode_WhenNumericStringHasLeadingOrTrailingWhitespace(string code, int expected)
    {
        // Act
        LocalAuthorityCode result = new(code);

        // Assert
        Assert.Equal(expected, result.Code);
    }

    [Theory]
    [InlineData("+1")]
    [InlineData("1.0")]
    [InlineData("1,000")]
    [InlineData("1e2")]
    [InlineData("0x10")]
    public void Constructor_ThrowsArgumentException_WhenCodeHasInvalidNumericFormat_EvenAfterTrimming(string code)
    {
        // Act Assert
        ArgumentException exception = Assert.Throws<ArgumentException>(() => new LocalAuthorityCode(code));
    }

    [Theory]
    [InlineData("-1")]
    [InlineData(" -1 ")]
    [InlineData("-100")]
    public void Constructor_ThrowsArgumentException_WhenParsedCodeIsNegative(string code)
    {
        // Act & Assert
        ArgumentException exception = Assert.ThrowsAny<ArgumentException>(() => new LocalAuthorityCode(code));
    }

    [Theory]
    [InlineData("1000")]
    [InlineData("1234")]
    [InlineData(" 1000 ")]
    [InlineData("2147483648")]
    [InlineData("999999999999")]
    [InlineData(" 2147483648 ")]
    public void Constructor_ThrowsArgumentException_WhenParsedCodeIsTooLarge(string code)
    {
        // Act & Assert
        ArgumentException exception = Assert.ThrowsAny<ArgumentException>(() => new LocalAuthorityCode(code));
    }
}
