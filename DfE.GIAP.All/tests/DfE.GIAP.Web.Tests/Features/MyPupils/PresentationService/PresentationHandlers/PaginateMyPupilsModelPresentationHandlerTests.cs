using DfE.GIAP.Web.Features.MyPupils.PresentationService;
using DfE.GIAP.Web.Features.MyPupils.PresentationService.Models;
using DfE.GIAP.Web.Features.MyPupils.PresentationService.PresentationHandlers;
using DfE.GIAP.Web.Features.MyPupils.SelectionState;
using DfE.GIAP.Web.Tests.Features.MyPupils.TestDoubles;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.PresentationService.PresentationHandlers;
public sealed class PaginateMyPupilsModelPresentationHandlerTests
{
    private const int DEFAULT_PAGE_SIZE = 20;

    [Fact]
    public void Handle_Returns_Empty_When_Pupils_Are_Empty()
    {
        // Arrange
        MyPupilsPresentationQueryModel query = MyPupilsPresentationQueryModel.CreateDefault();

        MyPupilsPresentationPupilModels pupils = MyPupilsPresentationPupilModels.Empty();

        PaginateMyPupilsModelPresentationHandler sut = new();

        // Act
        MyPupilsPresentationPupilModels response = sut.Handle(pupils, query, It.IsAny<MyPupilsPupilSelectionState>());

        // Assert
        Assert.NotNull(response);
        Assert.Empty(response.Values);
    }


    [Theory]
    [InlineData(10)]
    [InlineData(DEFAULT_PAGE_SIZE)]
    public void Handle_Returns_Pupils_Up_To_PageSize(int pupilCount)
    {
        // Arrange
        MyPupilsPresentationQueryModel query = MyPupilsPresentationQueryModelTestDoubles.Create(page: 1);

        MyPupilsPresentationPupilModels pupils = MyPupilsPresentationPupilModelsTestDoubles.Generate(pupilCount);

        PaginateMyPupilsModelPresentationHandler sut = new();

        // Act
        MyPupilsPresentationPupilModels response = sut.Handle(pupils, query, It.IsAny<MyPupilsPupilSelectionState>());

        // Assert
        Assert.NotNull(response);
        Assert.Equivalent(pupils, response);
    }


    [Fact]
    public void Handle_Returns_PageOfPupils_When_PageNumber_Requested()
    {
        // Arrange
        MyPupilsPresentationQueryModel query = MyPupilsPresentationQueryModelTestDoubles.Create(page: 2);

        MyPupilsPresentationPupilModels pupils = MyPupilsPresentationPupilModelsTestDoubles.Generate(DEFAULT_PAGE_SIZE);

        PaginateMyPupilsModelPresentationHandler sut = new();

        // Act
        MyPupilsPresentationPupilModels response = sut.Handle(pupils, query, It.IsAny<MyPupilsPupilSelectionState>());

        // Assert
        Assert.NotNull(response);
        Assert.Empty(response.Values);
    }

    [Fact]
    public void Handle_Returns_PartialPage_Of_Pupils_When_PageNumber_Requested()
    {
        // Arrange
        const int fullPagesOfPupils = 4;

        const int partialPageOfPupilsCount = DEFAULT_PAGE_SIZE * fullPagesOfPupils + 3;

        MyPupilsPresentationQueryModel presentationState =
            MyPupilsPresentationQueryModelTestDoubles.Create(page: fullPagesOfPupils + 1); // request next page

        MyPupilsPresentationPupilModels pupils = MyPupilsPresentationPupilModelsTestDoubles.Generate(partialPageOfPupilsCount);

        PaginateMyPupilsModelPresentationHandler sut = new();

        // Act
        MyPupilsPresentationPupilModels response = sut.Handle(pupils, presentationState, It.IsAny<MyPupilsPupilSelectionState>());

        // Assert
        IEnumerable<MyPupilsPresentationPupilModel> expectedPagedPupils = pupils.Values.Skip(DEFAULT_PAGE_SIZE * fullPagesOfPupils);

        Assert.NotNull(response);
        Assert.Equal(3, response.Count);
        Assert.Equivalent(expectedPagedPupils, response.Values);
    }
}

