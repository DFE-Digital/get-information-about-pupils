using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.SharedTests.Features.MyPupils.Application;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection.GetPupilSelections;
using DfE.GIAP.Web.Features.MyPupils.Services.GetSelectedPupilUpns;
using DfE.GIAP.Web.Tests.Features.MyPupils.TestDoubles;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.Services;
public sealed class GetSelectedPupilsUniquePupilNumbersPresentationServiceTests
{
    [Fact]
    public async Task GetSelectedPupilsAsync_Returns_AllPupils_Except_Deselections_When_SelectionMode_Is_All()
    {
        // Arrange
        Mock<IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>> useCaseMock = new();

        GetMyPupilsResponse useCaseResponse = new(MyPupilsModelTestDoubles.Generate(count: 10));
        useCaseMock
            .Setup((useCase) => useCase.HandleRequestAsync(It.IsAny<GetMyPupilsRequest>()))
            .ReturnsAsync(useCaseResponse);

        // Deselect a few pupils out of all pupils
        List<string> deselectedPupils = useCaseResponse.MyPupils.Identifiers.TakeLast(2).ToList();
        MyPupilsPupilSelectionState selectionStateStub =
            MyPupilsPupilSelectionStateTestDoubles.WithSelectedPupils(
                mode: SelectionMode.All,
                selected: useCaseResponse.MyPupils.Identifiers.ToList(),
                deselected: deselectedPupils);

        Mock<IGetMyPupilsPupilSelectionProvider> getPupilSelectionsMock = new();
        getPupilSelectionsMock
            .Setup(t => t.GetPupilSelections())
            .Returns(selectionStateStub);

        GetSelectedPupilsUniquePupilNumbersPresentationService sut = new(getPupilSelectionsMock.Object, useCaseMock.Object);

        // Act
        const string userId = "userId";
        IEnumerable<string> response = await sut.GetSelectedPupilsAsync(userId);

        // Assert
        Assert.NotNull(response);
        Assert.Equivalent(response, useCaseResponse.MyPupils.Identifiers.Except(deselectedPupils));

        getPupilSelectionsMock.Verify(
            (provider) => provider.GetPupilSelections(),
                Times.Once);

        useCaseMock.Verify(
            (useCase) => useCase.HandleRequestAsync(It.Is<GetMyPupilsRequest>(request => request.UserId == userId)),
                Times.Once);
    }

    [Fact]
    public async Task GetSelectedPupilsAsync_Returns_ExplicitSelections_When_SelectionMode_Is_Not_All()
    {
        // Arrange
        List<string> explicitSelections = ["a", "b", "c"];

        // TODO back behind TestDouble/Builder
        MyPupilsPupilSelectionState selectionStateStub =
            MyPupilsPupilSelectionStateTestDoubles.WithSelectedPupils(
                mode: SelectionMode.Manual,
                selected: explicitSelections,
                deselected: []);

        Mock<IGetMyPupilsPupilSelectionProvider> getPupilSelectionsMock = new();
        getPupilSelectionsMock
            .Setup(t => t.GetPupilSelections())
            .Returns(selectionStateStub);


        GetSelectedPupilsUniquePupilNumbersPresentationService sut = new(
            getPupilSelectionsMock.Object,
            new Mock<IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>>().Object);

        // Act
        const string userId = "userId";
        IEnumerable<string> response = await sut.GetSelectedPupilsAsync(userId);

        // Assert
        Assert.NotNull(response);
        Assert.Equivalent(response, explicitSelections);

        getPupilSelectionsMock.Verify(
            (provider) => provider.GetPupilSelections(),
                Times.Once);
    }
}
