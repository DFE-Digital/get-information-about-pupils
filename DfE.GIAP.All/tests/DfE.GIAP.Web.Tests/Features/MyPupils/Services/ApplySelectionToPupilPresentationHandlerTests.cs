namespace DfE.GIAP.Web.Tests.Features.MyPupils.Services;


// TODO MOVE INTO GETPRESENTATIONSERVICE TESTS

/*public sealed class ApplySelectionToPupilPresentationHandlerTests
{
    [Fact]
    public async Task Handle_With_Null_Returns_Empty_Pupils()
    {
        // Arrange
        ApplySelectionToPupilPresentationHandler sut = new();

        // Act
        HandlerResult<MyPupilsPresentationPupilModels> response = await sut.HandleAsync(null!);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Result);
        Assert.Empty(response.Result.Values);
    }

    [Fact]
    public async Task Handle_Pupils_IsSelected_Set_False_When_NotSelected()
    {
        // Arrange
        ApplySelectionToPupilPresentationHandler sut = new();

        MyPupilsPresentationPupilModels pupils = MyPupilsPresentationPupilModelsTestDoubles.Generate(10);

        MyPupilsPresentationHandlerRequest request = new(
            pupils,
            It.IsAny<MyPupilsPresentationQueryModel>(),
            MyPupilsPupilSelectionState.CreateDefault());

        // Act
        HandlerResult<MyPupilsPresentationPupilModels> response = await sut.HandleAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Result);
        Assert.Equal(pupils.Count, response.Result.Count);

        Assert.All(response.Result.Values, (pupil) =>
        {
            Assert.False(pupil.IsSelected);
        });
    }

    [Fact]
    public async Task Handle_Selected_Pupil_IsSelected()
    {
        ApplySelectionToPupilPresentationHandler sut = new();

        MyPupilsPresentationPupilModels pupils = MyPupilsPresentationPupilModelsTestDoubles.Generate(10);

        List<string> selectedPupils = [pupils.Values[0].UniquePupilNumber, pupils.Values[2].UniquePupilNumber];

        MyPupilsPupilSelectionState selectionState =
            MyPupilsPupilSelectionStateTestDoubles.WithSelectedPupils(selectedPupils);

        MyPupilsPresentationHandlerRequest request = new(
            pupils,
            It.IsAny<MyPupilsPresentationQueryModel>(),
            selectionState);

        // Act
        HandlerResult<MyPupilsPresentationPupilModels> response = await sut.HandleAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Result);
        Assert.Equal(pupils.Count, response.Result.Count);

        Assert.Equal(
            selectedPupils,
            response.Result.Values
                .Where(pupil => selectionState.IsPupilSelected(pupil.UniquePupilNumber))
                .Select(t => t.UniquePupilNumber));
    }
}
*/
