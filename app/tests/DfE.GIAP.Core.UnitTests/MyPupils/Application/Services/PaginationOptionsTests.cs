using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.QueryModel;

namespace DfE.GIAP.Core.UnitTests.MyPupils.Application.Services;

public sealed class PaginationOptionsTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void Constructor_Throws_When_Size_Is_LessThanOrEqualZero(int invalidSize)
    {
        // Arrange
        int validPage = 1;

        // Act
        Func<PaginationOptions> act = () => new(validPage, invalidSize);

        // Assert
        ArgumentOutOfRangeException ex = Assert.Throws<ArgumentOutOfRangeException>(act);
        Assert.Equal("resultsSize", ex.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void Constructor_Throws_When_Page_Is_LessThanOne(int invalidPage)
    {
        // Arrange
        const int validSize = 10;

        // Act
        Func<PaginationOptions> act = () => new(invalidPage, validSize);

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void Constructor_Succeeds_For_Minimal_Valid_Page_And_Size()
    {
        // Arrange
        const int page = 1;
        const int size = 1;

        // Act
        PaginationOptions options = new(page, size);

        // Assert
        Assert.NotNull(options);
        Assert.Equal(page, options.Page.Value);
        Assert.Equal(size, options.Size);
    }

    [Fact]
    public void Constructor_Succeeds_For_Large_Valid_Page_And_Size()
    {
        // Arrange
        int page = int.MaxValue;
        int size = int.MaxValue;

        // Act
        PaginationOptions options = new(page, size);

        // Assert
        Assert.NotNull(options);
        Assert.Equal(page, options.Page.Value);
        Assert.Equal(size, options.Size);
    }

    [Theory]
    [InlineData(1, 10)]
    [InlineData(2, 25)]
    [InlineData(10, 50)]
    public void Properties_Are_Correctly_Assigned(int page, int size)
    {
        // Arrange & Act
        PaginationOptions options = new(page, size);

        // Assert
        Assert.Equal(page, options.Page.Value);
        Assert.Equal(size, options.Size);
    }
}
