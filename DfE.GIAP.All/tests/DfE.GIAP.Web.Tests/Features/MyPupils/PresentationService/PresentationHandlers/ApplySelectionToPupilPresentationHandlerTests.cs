using DfE.GIAP.Web.Features.MyPupils.PresentationService;
using DfE.GIAP.Web.Features.MyPupils.PresentationService.Models;
using DfE.GIAP.Web.Features.MyPupils.PresentationService.PresentationHandlers;
using DfE.GIAP.Web.Features.MyPupils.SelectionState;
using DfE.GIAP.Web.Tests.Features.MyPupils.TestDoubles;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.PresentationService.PresentationHandlers;
public sealed class ApplySelectionToPupilPresentationHandlerTests
{
    [Fact]
    public void Handle_With_Null_Pupils_Returns_Empty_Pupils()
    {
        // Arrange
        ApplySelectionToPupilPresentationHandler sut = new();

        // Act
        MyPupilsPresentationPupilModels response = sut.Handle(
            null!, It.IsAny<MyPupilsPresentationQueryModel>(), It.IsAny<MyPupilsPupilSelectionState>());

        // Assert
        Assert.NotNull(response);
        Assert.Empty(response.Values);
    }

    [Fact]
    public void Handle_Pupils_Are_IsSelected_False_When_SelectionState_Is_Null()
    {
        // Arrange
        ApplySelectionToPupilPresentationHandler sut = new();

        MyPupilsPresentationPupilModels pupils = MyPupilsPresentationPupilModelsTestDoubles.Generate(10);

        // Act
        MyPupilsPresentationPupilModels response =
            sut.Handle(
                pupils,
                It.IsAny<MyPupilsPresentationQueryModel>(),
                null);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(pupils.Count, response.Count);

        Assert.All(response.Values, (pupil) =>
        {
            Assert.False(pupil.IsSelected);
        });
    }

    [Fact]
    public void Handle_Selected_Pupil_IsSelected()
    {
        ApplySelectionToPupilPresentationHandler sut = new();

        MyPupilsPresentationPupilModels pupils = MyPupilsPresentationPupilModelsTestDoubles.Generate(10);

        List<string> selectedPupils = [pupils.Values[0].UniquePupilNumber, pupils.Values[2].UniquePupilNumber];

        MyPupilsPupilSelectionState selectionState =
            MyPupilsPupilSelectionStateTestDoubles.WithSelectedPupils(selectedPupils);

        // Act
        MyPupilsPresentationPupilModels response =
            sut.Handle(pupils, It.IsAny<MyPupilsPresentationQueryModel>(), selectionState);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(pupils.Count, response.Count);

        Assert.Equal(
            selectedPupils,
            response.Values
                .Where(pupil => selectionState.IsPupilSelected(pupil.UniquePupilNumber))
                .Select(t => t.UniquePupilNumber));
    }
}
