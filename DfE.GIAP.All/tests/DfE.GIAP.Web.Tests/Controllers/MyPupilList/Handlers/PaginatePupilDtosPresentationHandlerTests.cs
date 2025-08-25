using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.Web.Controllers.MyPupilList.Handlers.GetPupilsForUserFromPresentationState.PupilDtoPresentationHandlers;
using DfE.GIAP.Web.Controllers.MyPupilList.PresentationState;
using DfE.GIAP.Web.Tests.Controllers.MyPupilList.TestDoubles;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Controllers.MyPupilList.Handlers;
public sealed class PaginatePupilDtosPresentationHandlerTests
{
    private const int DEFAULT_PAGE_SIZE = 20;

    [Fact]
    public void Handle_Throws_When_PageNumber_Is_LessThan_1()
    {
        MyPupilsPresentationState options = MyPupilsPresentationStateTestDoubles.Create(page: 0);

        PaginatePupilDtosPresentationHandler sut = new();

        Action act = () => sut.Handle(It.IsAny<IEnumerable<PupilDto>>(), options);

        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void Handle_Returns_Empty_When_Pupils_Are_Empty()
    {
        // Arrange
        MyPupilsPresentationState options = MyPupilsPresentationStateTestDoubles.CreateWithValidPage();

        List<PupilDto> pupils = [];

        PaginatePupilDtosPresentationHandler sut = new();

        // Act
        IEnumerable<PupilDto> response = sut.Handle(pupils, options);

        // Assert
        Assert.NotNull(response);
        Assert.Empty(response);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(DEFAULT_PAGE_SIZE)]
    public void Handle_Returns_Pupils_Up_To_PageSize(int pupilDtosCount)
    {
        // Arrange
        MyPupilsPresentationState options = MyPupilsPresentationStateTestDoubles.Create(page: 1);

        List<PupilDto> pupils = PupilDtoTestDoubles.Generate(count: pupilDtosCount);

        PaginatePupilDtosPresentationHandler sut = new();

        // Act
        IEnumerable<PupilDto> response = sut.Handle(pupils, options);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(pupils, response);
    }

    [Fact]
    public void Handle_Returns_PageOfPupils_When_PageNumber_Requested()
    {
        // Arrange
        MyPupilsPresentationState options = MyPupilsPresentationStateTestDoubles.Create(page: 2);

        List<PupilDto> pupils = PupilDtoTestDoubles.Generate(count: DEFAULT_PAGE_SIZE);

        PaginatePupilDtosPresentationHandler sut = new();

        // Act
        IEnumerable<PupilDto> response = sut.Handle(pupils, options);

        // Assert
        Assert.NotNull(response);
        Assert.Empty(response);
    }

    [Fact]
    public void Handle_Returns_PartialPage_Of_Pupils_When_PageNumber_Requested()
    {
        // Arrange
        const int fullPagesOfPupils = 4;
        const int pageRequested = fullPagesOfPupils + 1;
        MyPupilsPresentationState options = MyPupilsPresentationStateTestDoubles.Create(page: pageRequested);

        int inputPupilCount = DEFAULT_PAGE_SIZE * fullPagesOfPupils + 3;
        List<PupilDto> pupils = PupilDtoTestDoubles.Generate(count: inputPupilCount);

        PaginatePupilDtosPresentationHandler sut = new();

        // Act
        IEnumerable<PupilDto> response = sut.Handle(pupils, options);

        // Assert
        IEnumerable<PupilDto> expectedPagedPupils = pupils.Skip(DEFAULT_PAGE_SIZE * fullPagesOfPupils);
        Assert.Equal(3, response.Count());
        Assert.Equal(response, expectedPagedPupils);
    }
}
