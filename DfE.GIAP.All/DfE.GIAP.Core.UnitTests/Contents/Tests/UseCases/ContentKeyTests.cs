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
        Action construct = () => ContentKey.Create(invalidId);

        // Act Assert
        Assert.Throws<ArgumentException>(construct);
    }
}
