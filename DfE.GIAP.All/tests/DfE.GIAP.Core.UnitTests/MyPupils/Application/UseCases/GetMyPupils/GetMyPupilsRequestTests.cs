using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Request;

namespace DfE.GIAP.Core.UnitTests.MyPupils.Application.UseCases.GetMyPupils;
public sealed class GetMyPupilsRequestTests
{
    [Theory]
    [InlineData(SortDirection.Ascending)]
    [InlineData(SortDirection.Descending)]
    public void Constructor_SetsUserIdAndOptions(SortDirection sortDirection)
    {
        // Arrange
        const string userId = "user";

        MyPupilsQueryOptions options = new(
            new OrderPupilsBy("Surname", sortDirection),
            PageNumber.Page(2));

        // Act
        GetMyPupilsRequest request = new(userId, options);

        // Assert
        Assert.Equal(userId, request.UserId);
        Assert.Equal("Surname", request.Options!.Order.Field);
        Assert.Equal(sortDirection, request.Options.Order.Direction);
        Assert.Equal(2, request.Options.Page.Value);
    }

    [Fact]
    public void Constructor_AllowsNullOptions()
    {
        // Arrange
        const string userId = "user";

        // Act
        GetMyPupilsRequest request = new(userId);

        // Assert
        Assert.Equal(userId, request.UserId);
        Assert.Null(request.Options);
    }
}
