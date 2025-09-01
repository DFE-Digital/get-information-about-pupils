using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.Web.Features.MyPupils.Handlers.GetPaginatedMyPupils.PresentationHandlers.Paginate;
using DfE.GIAP.Web.Features.MyPupils.State.Presentation;
using DfE.GIAP.Web.Tests.Features.MyPupilList.TestDoubles;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupilList.GetPaginatedMyPupils;
public sealed class PaginatePupilDtosPresentationHandlerTests
{
    private const int DEFAULT_PAGE_SIZE = 20;

    [Fact]
    public void Handle_Throws_When_PageNumber_Is_LessThan_1()
    {
        MyPupilsPresentationState options = MyPupilsPresentationStateTestDoubles.Create(page: 0);

        PaginatePupilDtosPresentationHandler sut = new();

        Action act = () => sut.Handle(It.IsAny<PupilDtos>(), options);

        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void Handle_Returns_Empty_When_Pupils_Are_Empty()
    {
        // Arrange
        MyPupilsPresentationState options = MyPupilsPresentationStateTestDoubles.CreateWithValidPage();

        PupilDtos pupils = PupilDtos.Empty();

        PaginatePupilDtosPresentationHandler sut = new();

        // Act
        PupilDtos response = sut.Handle(pupils, options);

        // Assert
        Assert.NotNull(response.Pupils);
        Assert.Empty(response.Pupils);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(DEFAULT_PAGE_SIZE)]
    public void Handle_Returns_Pupils_Up_To_PageSize(int pupilDtosCount)
    {
        // Arrange
        MyPupilsPresentationState options = MyPupilsPresentationStateTestDoubles.Create(page: 1);

        PupilDtos pupils = PupilDtoTestDoubles.Generate(count: pupilDtosCount);

        PaginatePupilDtosPresentationHandler sut = new();

        // Act
        PupilDtos response = sut.Handle(pupils, options);

        // Assert
        Assert.NotNull(response.Pupils);
        Assert.Equal(pupils.Pupils, response.Pupils);
    }

    [Fact]
    public void Handle_Returns_PageOfPupils_When_PageNumber_Requested()
    {
        // Arrange
        MyPupilsPresentationState options = MyPupilsPresentationStateTestDoubles.Create(page: 2);

        PupilDtos pupils = PupilDtoTestDoubles.Generate(count: DEFAULT_PAGE_SIZE);

        PaginatePupilDtosPresentationHandler sut = new();

        // Act
        PupilDtos response = sut.Handle(pupils, options);

        // Assert
        Assert.NotNull(response.Pupils);
        Assert.Empty(response.Pupils);
    }

    [Fact]
    public void Handle_Returns_PartialPage_Of_Pupils_When_PageNumber_Requested()
    {
        // Arrange
        const int fullPagesOfPupils = 4;
        const int pageRequested = fullPagesOfPupils + 1;
        MyPupilsPresentationState options = MyPupilsPresentationStateTestDoubles.Create(page: pageRequested);

        int inputPupilCount = DEFAULT_PAGE_SIZE * fullPagesOfPupils + 3;
        PupilDtos pupilDtos = PupilDtoTestDoubles.Generate(count: inputPupilCount);

        PaginatePupilDtosPresentationHandler sut = new();

        // Act
        PupilDtos response = sut.Handle(pupilDtos, options);

        // Assert
        IEnumerable<PupilDto> expectedPagedPupils = pupilDtos.Pupils.Skip(DEFAULT_PAGE_SIZE * fullPagesOfPupils);
        Assert.Equal(3, response.Count);
        Assert.Equal(response.Pupils, expectedPagedPupils);
    }
}
