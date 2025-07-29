using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.UnitTests.MyPupils.Domain.ValueObjects;
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
}
