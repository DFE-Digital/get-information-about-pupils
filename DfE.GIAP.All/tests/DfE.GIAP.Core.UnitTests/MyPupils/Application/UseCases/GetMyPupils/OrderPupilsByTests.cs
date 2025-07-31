using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Request;

namespace DfE.GIAP.Core.UnitTests.MyPupils.Application.UseCases.GetMyPupils;
public class OrderPupilsByTests
{
    [Fact]
    public void Constructor_SetsFieldAndDirection()
    {
        // Act
        OrderPupilsBy order = new(field: "Forename", direction: SortDirection.Ascending);

        // Assert
        Assert.Equal("Forename", order.Field);
        Assert.Equal(SortDirection.Ascending, order.Direction);
    }

    [Fact]
    public void Constructor_AllowsNullField_AndDefaultsToEmpty()
    {
        // Act
        OrderPupilsBy order = new(field: null!, direction: SortDirection.Descending);

        // Assert
        Assert.Equal(string.Empty, order.Field);
        Assert.Equal(SortDirection.Descending, order.Direction);
    }
}
