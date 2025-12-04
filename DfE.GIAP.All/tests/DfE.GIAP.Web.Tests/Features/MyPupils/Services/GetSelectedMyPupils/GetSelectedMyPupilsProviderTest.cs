using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.Web.Features.MyPupils.Services.GetPupilViewModels.GetSelectedMyPupils;
using DfE.GIAP.Web.Features.MyPupils.State.Selection;
using DfE.GIAP.Web.Session.Abstraction.Query;
using DfE.GIAP.Web.Tests.TestDoubles.MyPupils;
using DfE.GIAP.Web.Tests.TestDoubles.Session;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.Services.GetSelectedMyPupils;
public sealed class GetSelectedMyPupilsProviderTest
{
    [Fact]
    public void Constructor_Throws_When_SessionQueryHandler_Is_Null()
    {
        // Arrange
        Func<GetSelectedMyPupilsHandler> construct =
            () => new GetSelectedMyPupilsHandler(null!);

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

        GetSelectedMyPupilsHandler provider = new(sessionQueryHandlerMock.Object);

        // Act
        IEnumerable<string> result = provider.GetSelectedMyPupils();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetSelectedMyPupils_ReturnsOnlySelectedPupils()
    {
        // Arrange
        List<UniquePupilNumber> uniquePupilsNumbers = UniquePupilNumberTestDoubles.Generate(2);
        UniquePupilNumber selected = uniquePupilsNumbers[0];
        UniquePupilNumber notSelected = uniquePupilsNumbers[1];

        Dictionary<List<string>, bool> selection = new()
        {
            { [selected.Value], true },
            { [notSelected.Value ], false }
        };

        MyPupilsPupilSelectionState state = MyPupilsPupilSelectionStateTestDoubles.WithPupilsSelectionState(selection);

        // Arrange
        Mock<ISessionQueryHandler<MyPupilsPupilSelectionState>> sessionQueryHandlerMock =
            ISessionQueryHandlerTestDoubles.MockFor(
                SessionQueryResponse<MyPupilsPupilSelectionState>.Create(
                    value: state));

        GetSelectedMyPupilsHandler provider = new(sessionQueryHandlerMock.Object);

        // Act
        IEnumerable<string> result = provider.GetSelectedMyPupils();

        // Assert
        string responseUniquePupilNumber = Assert.Single(result);
        Assert.Equal(selected.Value, responseUniquePupilNumber);
        Assert.DoesNotContain(notSelected.Value, result);
    }
}
