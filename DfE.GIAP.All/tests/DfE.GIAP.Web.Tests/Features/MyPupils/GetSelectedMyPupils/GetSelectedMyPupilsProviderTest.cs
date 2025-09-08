using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.Web.Features.MyPupils.GetSelectedMyPupils;
using DfE.GIAP.Web.Features.MyPupils.State.Selection;
using DfE.GIAP.Web.Session.Abstraction.Query;
using DfE.GIAP.Web.Tests.TestDoubles.MyPupils;
using DfE.GIAP.Web.Tests.TestDoubles.Session;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.GetSelectedMyPupils;
public sealed class GetSelectedMyPupilsProviderTest
{
    [Fact]
    public void Constructor_Throws_When_SessionQueryHandler_Is_Null()
    {
        // Arrange
        Func<GetSelectedMyPupilsProvider> construct =
            () => new GetSelectedMyPupilsProvider(null!);

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

        GetSelectedMyPupilsProvider provider = new(sessionQueryHandlerMock.Object);

        // Act
        UniquePupilNumbers result = provider.GetSelectedMyPupils();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.GetUniquePupilNumbers());
    }

    [Fact]
    public void GetSelectedMyPupils_ReturnsOnlySelectedPupils()
    {
        // Arrange
        List<UniquePupilNumber> uniquePupilsNumbers = UniquePupilNumberTestDoubles.Generate(2);
        UniquePupilNumber selected = uniquePupilsNumbers[0];
        UniquePupilNumber notSelected = uniquePupilsNumbers[1];

        Dictionary<List<UniquePupilNumber>, bool> selection = new()
        {
            { [selected], true },
            { [notSelected], false }
        };

        MyPupilsPupilSelectionState state = MyPupilsPupilSelectionStateTestDoubles.WithSelectionState(selection);

        // Arrange
        Mock<ISessionQueryHandler<MyPupilsPupilSelectionState>> sessionQueryHandlerMock =
            ISessionQueryHandlerTestDoubles.MockFor(
                SessionQueryResponse<MyPupilsPupilSelectionState>.Create(
                    value: state));

        GetSelectedMyPupilsProvider provider = new(sessionQueryHandlerMock.Object);

        // Act
        UniquePupilNumbers result = provider.GetSelectedMyPupils();

        // Assert
        UniquePupilNumber responseUniquePupilNumber = Assert.Single(result.GetUniquePupilNumbers());
        Assert.Equal(selected, responseUniquePupilNumber);
        Assert.DoesNotContain(notSelected, result.GetUniquePupilNumbers());
    }
}

