using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Request;

namespace DfE.GIAP.Core.UnitTests.MyPupils.Application.UseCases.GetMyPupils;
public sealed class MyPupilsQueryOptionsTests
{
    [Fact]
    public void Default_ReturnsExpectedDefaults()
    {
        // Act
        MyPupilsQueryOptions result = MyPupilsQueryOptions.Default();

        // Assert
        OrderPupilsBy expectedOrder = new(field: string.Empty, SortDirection.Descending);
        Assert.Equal(expectedOrder, result.Order);
        Assert.Equal(PageNumber.Default, result.Page);
    }
}
