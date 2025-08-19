using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PresenterHandler.Options;
using DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PresenterHandler.Order;
using DfE.GIAP.Web.Tests.Controllers.MyPupils.TestDoubles;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Controllers.MyPupils;
public sealed class OrderPupilDtosPresentationHandlerTests
{
    [Fact]
    public void Test()
    {
        // Arrange
        PupilsPresentationOptions options = PupilPresentationOptionsTestDoubles.Create(sortKey: string.Empty);

        List<PupilDto> pupils = PupilDtoTestDoubles.Generate(count: 10);

        OrderPupilDtosPresentationHandler sut = new();

        // Act
        IEnumerable<PupilDto> response = sut.Handle(pupils, options);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(pupils, response);
    }

    [Fact]
    public void Test2()
    {
        // Arrange
        PupilsPresentationOptions options = PupilPresentationOptionsTestDoubles.Create(sortKey: "unknown-sortByKey");

        OrderPupilDtosPresentationHandler sut = new();

        // Act Assert
        Action act = () => sut.Handle(It.IsAny<IEnumerable<PupilDto>>(), options);
        Assert.Throws<ArgumentException>(act);
    }

    [Theory]
    [InlineData("forename", SortDirection.Ascending)]
    [InlineData("forename", SortDirection.Descending)]
    [InlineData("FORENAME", SortDirection.Ascending)]
    [InlineData("foRENamE", SortDirection.Descending)]
    public void Test3(string sortKey, SortDirection sortDirection)
    {
        // Arrange
        PupilsPresentationOptions options = PupilPresentationOptionsTestDoubles.Create(sortKey, sortDirection);

        IEnumerable<PupilDto> pupils = PupilDtoTestDoubles.Generate(count: 20);

        OrderPupilDtosPresentationHandler sut = new();

        // Act
        IEnumerable<PupilDto> response = sut.Handle(pupils, options);

        // Assert
        IEnumerable<PupilDto> expected =
            sortDirection == SortDirection.Ascending ?
                pupils.OrderBy(t => t.Forename) :
                pupils.OrderByDescending(t => t.Forename);

        Assert.Equal(expected, response);
    }

    [Theory]
    [InlineData("surname", SortDirection.Ascending)]
    [InlineData("surname", SortDirection.Descending)]
    [InlineData("SURNAME", SortDirection.Ascending)]
    [InlineData("suRnAme", SortDirection.Descending)]
    public void Test4(string sortKey, SortDirection sortDirection)
    {
        // Arrange
        PupilsPresentationOptions options = PupilPresentationOptionsTestDoubles.Create(sortKey, sortDirection);

        IEnumerable<PupilDto> pupils = PupilDtoTestDoubles.Generate(count: 20);

        OrderPupilDtosPresentationHandler sut = new();

        // Act
        IEnumerable<PupilDto> response = sut.Handle(pupils, options);
        IEnumerable<PupilDto> expected =
            sortDirection == SortDirection.Ascending ?
                pupils.OrderBy(t => t.Surname) :
                pupils.OrderByDescending(t => t.Surname);

        // Assert
        Assert.Equal(expected, response);
    }

    [Theory]
    [InlineData("dob", SortDirection.Ascending)]
    [InlineData("dob", SortDirection.Descending)]
    [InlineData("DOB", SortDirection.Ascending)]
    [InlineData("dOB", SortDirection.Descending)]
    public void Test5(string sortKey, SortDirection sortDirection)
    {
        // Arrange
        PupilsPresentationOptions options = PupilPresentationOptionsTestDoubles.Create(sortKey, sortDirection);

        IEnumerable<PupilDto> pupils = PupilDtoTestDoubles.Generate(count: 20);

        OrderPupilDtosPresentationHandler sut = new();

        // Act
        IEnumerable<PupilDto> response = sut.Handle(pupils, options);

        // Assert
        IEnumerable<PupilDto> expected =
            sortDirection == SortDirection.Ascending ?
                pupils.OrderBy(t => t.ParseDateOfBirth()) :
                pupils.OrderByDescending(t => t.ParseDateOfBirth());

        Assert.Equal(expected, response);
    }

    [Theory]
    [InlineData("sex", SortDirection.Ascending)]
    [InlineData("sex", SortDirection.Descending)]
    [InlineData("SEX", SortDirection.Ascending)]
    [InlineData("seX", SortDirection.Descending)]
    public void Test6(string sortKey, SortDirection sortDirection)
    {
        // Arrange
        PupilsPresentationOptions options = PupilPresentationOptionsTestDoubles.Create(sortKey, sortDirection);

        IEnumerable<PupilDto> pupils = PupilDtoTestDoubles.Generate(count: 20);

        OrderPupilDtosPresentationHandler sut = new();

        // Act
        IEnumerable<PupilDto> response = sut.Handle(pupils, options);

        // Assert
        IEnumerable<PupilDto> expected =
            sortDirection == SortDirection.Ascending ?
                pupils.OrderBy(t => t.Sex) :
                pupils.OrderByDescending(t => t.Sex);

        Assert.Equal(expected, response);
    }
}

