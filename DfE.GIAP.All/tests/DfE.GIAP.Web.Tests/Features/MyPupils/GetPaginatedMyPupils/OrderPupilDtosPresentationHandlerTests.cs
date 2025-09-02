using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.Web.Features.MyPupils.Handlers.GetPaginatedMyPupils.PresentationHandlers.Order;
using DfE.GIAP.Web.Features.MyPupils.State.Presentation;
using DfE.GIAP.Web.Tests.Features.MyPupils.TestDoubles;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.GetPaginatedMyPupils;

public sealed class OrderPupilDtosPresentationHandlerTests
{
    [Fact]
    public void Handle_SortBy_Empty_Returns_Unsorted_Pupils()
    {
        // Arrange
        MyPupilsPresentationState state = MyPupilsPresentationStateTestDoubles.Create(sortKey: string.Empty);

        MyPupilDtos pupils = MyPupilDtosTestDoubles.Generate(count: 10);

        OrderMyPupilDtosPresentationHandler sut = new();

        // Act
        MyPupilDtos response = sut.Handle(pupils, state);

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
        Action act = () => sut.Handle(It.IsAny<MyPupilDtos>(), state);
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

        MyPupilDtos pupils = MyPupilDtosTestDoubles.Generate(count: 20);

        OrderMyPupilDtosPresentationHandler sut = new();

        // Act
        MyPupilDtos response = sut.Handle(pupils, state);

        // Assert
        IEnumerable<MyPupilDto> expected =
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

        MyPupilDtos pupils = MyPupilDtosTestDoubles.Generate(count: 20);

        OrderMyPupilDtosPresentationHandler sut = new();

        // Act
        MyPupilDtos response = sut.Handle(pupils, state);

        IEnumerable<MyPupilDto> expected =
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

        MyPupilDtos pupils = MyPupilDtosTestDoubles.Generate(count: 20);

        OrderMyPupilDtosPresentationHandler sut = new();

        // Act
        MyPupilDtos response = sut.Handle(pupils, state);

        // Assert
        IEnumerable<MyPupilDto> expected =
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

        MyPupilDtos pupils = MyPupilDtosTestDoubles.Generate(count: 20);

        OrderMyPupilDtosPresentationHandler sut = new();

        // Act
        MyPupilDtos response = sut.Handle(pupils, presentationState);

        // Assert
        IEnumerable<MyPupilDto> expected =
            sortDirection == SortDirection.Ascending ?
                pupils.Values.OrderBy(t => t.Sex) :
                pupils.Values.OrderByDescending(t => t.Sex);

        Assert.Equal(expected, response.Values);
    }
}
