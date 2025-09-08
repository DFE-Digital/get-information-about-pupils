using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Web.Features.MyPupils.GetSelectedMyPupils;
using DfE.GIAP.Web.Features.MyPupils.State.Selection;
using DfE.GIAP.Web.Session.Abstraction.Query;
using DfE.GIAP.Web.Tests.TestDoubles.Session;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.GetSelectedMyPupils;
public sealed class GetSelectedMyPupilUniquePupilNumbersProviderTests
{
    [Fact]
    public void Constructor_Throws_When_SessionQueryHandler_Is_Null()
    {
        // Arrange
        Func<GetSelectedMyPupilUniquePupilNumbersProvider> construct =
            () => new GetSelectedMyPupilUniquePupilNumbersProvider(null!);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void GetSelectedMyPupils_ReturnsEmpty_WhenSessionQueryResponseHasNoValue()
    {
        // Arrange
        Mock<ISessionQueryHandler<MyPupilsPupilSelectionState>> sessionQueryHandlerMock =
            ISessionQueryHandlerTestDoubles.MockFor(
                SessionQueryResponse<MyPupilsPupilSelectionState>.NoValue());

        GetSelectedMyPupilUniquePupilNumbersProvider provider = new(sessionQueryHandlerMock.Object);

        // Act
        UniquePupilNumbers result = provider.GetSelectedMyPupils();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.GetUniquePupilNumbers());
    }

    //[Fact]
    //public void GetSelectedMyPupils_ReturnsOnlySelectedPupils()
    //{
    //    // Arrange
    //    UniquePupilNumber upn1 = new("1234567890");
    //    UniquePupilNumber upn2 = new("0987654321");

    //    Dictionary<UniquePupilNumber, bool> selection = new()
    //    {
    //        [upn1] = true,
    //        [upn2] = false
    //    };

    //    MyPupilsPupilSelectionState state = new(selection);

    //    SessionQueryResponse<MyPupilsPupilSelectionState> response =
    //        new SessionQueryResponse<MyPupilsPupilSelectionState>(hasValue: true, value: state);

    //    Mock<ISessionQueryHandler<MyPupilsPupilSelectionState>> mockHandler =
    //        new Mock<ISessionQueryHandler<MyPupilsPupilSelectionState>>();

    //    mockHandler.Setup(h => h.GetSessionObject()).Returns(response);

    //    GetSelectedMyPupilUniquePupilNumbersProvider provider =
    //        new(mockHandler.Object);

    //    // Act
    //    UniquePupilNumbers result = provider.GetSelectedMyPupils();

    //    // Assert
    //    Assert.Single(result.Values);
    //    Assert.Contains(upn1, result.Values);
    //    Assert.DoesNotContain(upn2, result.Values);
    //}
}

