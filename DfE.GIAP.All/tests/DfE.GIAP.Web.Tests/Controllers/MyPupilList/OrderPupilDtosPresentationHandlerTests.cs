using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.Web.Features.MyPupils.Handlers.GetPaginatedMyPupils.PresentationHandlers.Order;
using DfE.GIAP.Web.Features.MyPupils.PresentationState;
using DfE.GIAP.Web.Tests.Controllers.MyPupilList.TestDoubles;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Controllers.MyPupilList;

public sealed class OrderPupilDtosPresentationHandlerTests
{
    [Fact]
    public void Handle_SortBy_Empty_Returns_Unsorted_Pupils()
    {
        // Arrange
        MyPupilsPresentationState options = PupilPresentationOptionsTestDoubles.Create(sortKey: string.Empty);

        List<PupilDto> pupils = PupilDtoTestDoubles.Generate(count: 10);

        OrderPupilDtosPresentationHandler sut = new();

        // Act
        IEnumerable<PupilDto> response = sut.Handle(pupils, options);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(pupils, response);
    }

    [Fact]
    public void Handle_SortBy_UnknownKey_Throws_ArgumentException()
    {
        // Arrange
        MyPupilsPresentationState options = PupilPresentationOptionsTestDoubles.Create(sortKey: "unknown-sortByKey");

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
    public void Handle_SortBy_Forename_Returns_SortedPupils_By_Forename(string sortKey, SortDirection sortDirection)
    {
        // Arrange
        MyPupilsPresentationState options = PupilPresentationOptionsTestDoubles.Create(sortKey, sortDirection);

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
    public void Handle_SortBy_Surname_Returns_SortedPupils_By_Surname(string sortKey, SortDirection sortDirection)
    {
        // Arrange
        MyPupilsPresentationState options = PupilPresentationOptionsTestDoubles.Create(sortKey, sortDirection);

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
    public void Handle_SortBy_DateOfBirth_Returns_SortedPupils_By_DateOfBirth(string sortKey, SortDirection sortDirection)
    {
        // Arrange
        MyPupilsPresentationState options = PupilPresentationOptionsTestDoubles.Create(sortKey, sortDirection);

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
    public void Handle_SortBy_Sex_Returns_SortedPupils_By_Sex(string sortKey, SortDirection sortDirection)
    {
        // Arrange
        MyPupilsPresentationState options = PupilPresentationOptionsTestDoubles.Create(sortKey, sortDirection);

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
