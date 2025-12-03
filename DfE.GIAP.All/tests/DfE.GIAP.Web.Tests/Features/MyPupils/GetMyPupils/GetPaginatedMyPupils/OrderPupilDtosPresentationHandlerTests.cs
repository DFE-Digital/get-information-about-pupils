using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.SharedTests.TestDoubles.MyPupils;
using DfE.GIAP.Web.Features.MyPupils.GetPaginatedMyPupils.PresentationHandlers.Order;
using DfE.GIAP.Web.Features.MyPupils.State.Presentation;
using DfE.GIAP.Web.Tests.TestDoubles.MyPupils;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.GetMyPupils.GetPaginatedMyPupils;

public sealed class OrderPupilDtosPresentationHandlerTests
{
    [Fact]
    public void Handle_SortBy_Empty_Returns_Unsorted_Pupils()
    {
        // Arrange
        MyPupilsPresentationState state = MyPupilsPresentationStateTestDoubles.Create(sortKey: string.Empty);

        MyPupilsModel pupils = MyPupilDtosTestDoubles.Generate(count: 10);

        OrderMyPupilDtosPresentationHandler sut = new();

        // Act
        MyPupilsModel response = sut.Handle(pupils, state);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(pupils, response);
    }

    [Fact]
    public void Handle_SortBy_UnknownKey_Throws_ArgumentException()
    {
        // Arrange
        MyPupilsPresentationState state = MyPupilsPresentationStateTestDoubles.Create(sortKey: "unknown-sortByKey");

        OrderMyPupilDtosPresentationHandler sut = new();

        // Act Assert
        Action act = () => sut.Handle(It.IsAny<MyPupilsModel>(), state);
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
        MyPupilsPresentationState state = MyPupilsPresentationStateTestDoubles.Create(sortKey, sortDirection);

        MyPupilsModel pupils = MyPupilDtosTestDoubles.Generate(count: 20);

        OrderMyPupilDtosPresentationHandler sut = new();

        // Act
        MyPupilsModel response = sut.Handle(pupils, state);

        // Assert
        IEnumerable<MyPupilModel> expected =
            sortDirection == SortDirection.Ascending ?
                pupils.Values.OrderBy(t => t.Forename) :
                pupils.Values.OrderByDescending(t => t.Forename);

        Assert.Equal(expected, response.Values);
    }

    [Theory]
    [InlineData("surname", SortDirection.Ascending)]
    [InlineData("surname", SortDirection.Descending)]
    [InlineData("SURNAME", SortDirection.Ascending)]
    [InlineData("suRnAme", SortDirection.Descending)]
    public void Handle_SortBy_Surname_Returns_SortedPupils_By_Surname(string sortKey, SortDirection sortDirection)
    {
        // Arrange
        MyPupilsPresentationState state = MyPupilsPresentationStateTestDoubles.Create(sortKey, sortDirection);

        MyPupilsModel pupils = MyPupilDtosTestDoubles.Generate(count: 20);

        OrderMyPupilDtosPresentationHandler sut = new();

        // Act
        MyPupilsModel response = sut.Handle(pupils, state);

        IEnumerable<MyPupilModel> expected =
            sortDirection == SortDirection.Ascending ?
                pupils.Values.OrderBy(t => t.Surname) :
                pupils.Values.OrderByDescending(t => t.Surname);

        // Assert
        Assert.Equal(expected, response.Values);
    }

    [Theory]
    [InlineData("dob", SortDirection.Ascending)]
    [InlineData("dob", SortDirection.Descending)]
    [InlineData("DOB", SortDirection.Ascending)]
    [InlineData("dOB", SortDirection.Descending)]
    public void Handle_SortBy_DateOfBirth_Returns_SortedPupils_By_DateOfBirth(string sortKey, SortDirection sortDirection)
    {
        // Arrange
        MyPupilsPresentationState state = MyPupilsPresentationStateTestDoubles.Create(sortKey, sortDirection);

        MyPupilsModel pupils = MyPupilDtosTestDoubles.Generate(count: 20);

        OrderMyPupilDtosPresentationHandler sut = new();

        // Act
        MyPupilsModel response = sut.Handle(pupils, state);

        // Assert
        IEnumerable<MyPupilModel> expected =
            sortDirection == SortDirection.Ascending ?
                pupils.Values.OrderBy(t => t.ParseDateOfBirth()) :
                pupils.Values.OrderByDescending(t => t.ParseDateOfBirth());

        Assert.Equal(expected, response.Values);
    }

    [Theory]
    [InlineData("sex", SortDirection.Ascending)]
    [InlineData("sex", SortDirection.Descending)]
    [InlineData("SEX", SortDirection.Ascending)]
    [InlineData("seX", SortDirection.Descending)]
    public void Handle_SortBy_Sex_Returns_SortedPupils_By_Sex(string sortKey, SortDirection sortDirection)
    {
        // Arrange
        MyPupilsPresentationState presentationState = MyPupilsPresentationStateTestDoubles.Create(sortKey, sortDirection);

        MyPupilsModel pupils = MyPupilDtosTestDoubles.Generate(count: 20);

        OrderMyPupilDtosPresentationHandler sut = new();

        // Act
        MyPupilsModel response = sut.Handle(pupils, presentationState);

        // Assert
        IEnumerable<MyPupilModel> expected =
            sortDirection == SortDirection.Ascending ?
                pupils.Values.OrderBy(t => t.Sex) :
                pupils.Values.OrderByDescending(t => t.Sex);

        Assert.Equal(expected, response.Values);
    }
}
