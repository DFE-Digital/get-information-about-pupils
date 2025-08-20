using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PresenterHandler.Options;
using DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PresenterHandler.Paginate;
using DfE.GIAP.Web.Tests.Controllers.MyPupils.TestDoubles;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Controllers.MyPupils;
public sealed class PaginatePupilDtosPresentationHandlerTests
{
    private const int DEFAULT_PAGE_SIZE = 20;

    [Fact]
    public void Test0()
    {
        PupilsPresentationOptions options = PupilPresentationOptionsTestDoubles.Create(page: 0);

        PaginatePupilDtosPresentationHandler sut = new();

        Action act = () => sut.Handle(It.IsAny<IEnumerable<PupilDto>>(), options);

        Assert.Throws<ArgumentOutOfRangeException>(act);
    }

    [Fact]
    public void Test1()
    {
        // Arrange
        PupilsPresentationOptions options = PupilPresentationOptionsTestDoubles.CreateWithValidPage();

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
    public void Test2(int pupilDtosCount)
    {
        // Arrange
        PupilsPresentationOptions options = PupilPresentationOptionsTestDoubles.Create(page: 1);

        List<PupilDto> pupils = PupilDtoTestDoubles.Generate(count: pupilDtosCount);

        PaginatePupilDtosPresentationHandler sut = new();

        // Act
        IEnumerable<PupilDto> response = sut.Handle(pupils, options);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(pupils, response);
    }

    [Fact]
    public void Test3()
    {
        // Arrange
        PupilsPresentationOptions options = PupilPresentationOptionsTestDoubles.Create(page: 2);

        List<PupilDto> pupils = PupilDtoTestDoubles.Generate(count: DEFAULT_PAGE_SIZE);

        PaginatePupilDtosPresentationHandler sut = new();

        // Act
        IEnumerable<PupilDto> response = sut.Handle(pupils, options);

        // Assert
        Assert.NotNull(response);
        Assert.Empty(response);
    }

    [Fact]
    public void Test4()
    {
        // Arrange
        const int fullPagesOfPupils = 4;
        const int pageRequested = fullPagesOfPupils + 1;
        PupilsPresentationOptions options = PupilPresentationOptionsTestDoubles.Create(page: pageRequested);

        int inputPupilCount = (DEFAULT_PAGE_SIZE * fullPagesOfPupils) + 3;
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
