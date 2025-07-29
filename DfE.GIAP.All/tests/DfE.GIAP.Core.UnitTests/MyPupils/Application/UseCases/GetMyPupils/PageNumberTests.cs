using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Request;

namespace DfE.GIAP.Core.UnitTests.MyPupils.Application.UseCases.GetMyPupils;
public class PageNumberTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_Throws_WhenPageIsLessThanOne(int invalidPage)
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new PageNumber(invalidPage));
    }

    [Fact]
    public void Constructor_SetsValue_WhenValid()
    {
        // Act
        PageNumber page = new(3);

        // Assert
        Assert.Equal(3, page.Value);
    }

    [Fact]
    public void Page_StaticMethod_ReturnsExpectedValue()
    {
        // Act
        PageNumber page = PageNumber.Page(5);

        // Assert
        Assert.Equal(5, page.Value);
    }
}
