using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.SharedTests.TestDoubles.MyPupils;
using DfE.GIAP.Web.Features.MyPupils.Services.GetPupilViewModels.Handlers.PresentationHandlers.Paginate;
using DfE.GIAP.Web.Features.MyPupils.State.Presentation;
using DfE.GIAP.Web.Tests.TestDoubles.MyPupils;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.Services.GetPupilViewModels.PresentationHandlers;
public sealed class PaginatePupilDtosPresentationHandlerTests
{
    private const int DEFAULT_PAGE_SIZE = 20;

    [Fact]
    public void Handle_Throws_When_PageNumber_Is_LessThan_1()
    {
        MyPupilsPresentationState presentationState = MyPupilsPresentationStateTestDoubles.Create(page: 0);

        PaginateMyPupilDtosPresentationHandler sut = new();

        Action act = () => sut.Handle(It.IsAny<MyPupilsModel>(), presentationState);

        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void Handle_Returns_Empty_When_Pupils_Are_Empty()
    {
        // Arrange
        MyPupilsPresentationState presentationState = MyPupilsPresentationStateTestDoubles.Default();

        MyPupilsModel pupils = MyPupilsModel.Empty();

        PaginateMyPupilDtosPresentationHandler sut = new();

        // Act
        MyPupilsModel response = sut.Handle(pupils, presentationState);

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
        MyPupilsPresentationState presentationState = MyPupilsPresentationStateTestDoubles.Create(page: 1);

        MyPupilsModel inputPupils = MyPupilDtosTestDoubles.Generate(count: pupilCount);

        PaginateMyPupilDtosPresentationHandler sut = new();

        // Act
        MyPupilsModel response = sut.Handle(inputPupils, presentationState);

        // Assert
        Assert.NotNull(response);
        Assert.Equivalent(inputPupils, response);
    }

    [Fact]
    public void Handle_Returns_PageOfPupils_When_PageNumber_Requested()
    {
        // Arrange
        MyPupilsPresentationState presentationState = MyPupilsPresentationStateTestDoubles.Create(page: 2);

        MyPupilsModel pupils = MyPupilDtosTestDoubles.Generate(count: DEFAULT_PAGE_SIZE);

        PaginateMyPupilDtosPresentationHandler sut = new();

        // Act
        MyPupilsModel response = sut.Handle(pupils, presentationState);

        // Assert
        Assert.NotNull(response);
        Assert.Empty(response.Values);
    }

    [Fact]
    public void Handle_Returns_PartialPage_Of_Pupils_When_PageNumber_Requested()
    {
        // Arrange
        const int fullPagesOfPupils = 4;
        const int pageRequested = fullPagesOfPupils + 1;
        MyPupilsPresentationState presentationState = MyPupilsPresentationStateTestDoubles.Create(page: pageRequested);

        int inputPupilCount = DEFAULT_PAGE_SIZE * fullPagesOfPupils + 3;
        MyPupilsModel pupils = MyPupilDtosTestDoubles.Generate(count: inputPupilCount);

        PaginateMyPupilDtosPresentationHandler sut = new();

        // Act
        MyPupilsModel response = sut.Handle(pupils, presentationState);

        // Assert
        IEnumerable<MyPupilModel> expectedPagedPupils = pupils.Values.Skip(DEFAULT_PAGE_SIZE * fullPagesOfPupils);

        Assert.NotNull(response);
        Assert.Equal(3, response.Count);
        Assert.Equivalent(expectedPagedPupils, response.Values);
    }
}
