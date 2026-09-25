using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils.Handlers;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.QueryModel;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.UnitTests.MyPupils.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.MyPupils.Application.Services;

public sealed class OrderPupilsHandlerTests
{
    [Fact]
    public void Order_Returns_Empty_When_Pupils_Is_Null()
    {
        // Arrange
        OrderOptions options = OrderOptions.Default();

        OrderPupilsHandler sut = new();

        // Act
        IEnumerable<Pupil> result = sut.Order(null!, options);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void Order_Returns_Empty_When_Pupils_Is_Empty()
    {
        // Arrange
        OrderOptions options = OrderOptions.Default();

        OrderPupilsHandler sut = new();

        // Act
        IEnumerable<Pupil> result = sut.Order([], options);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\n")]
    [InlineData(null)]
    public void Order_Returns_Unsorted_When_Field_Is_Null_Empty_Or_Whitespace(string? field)
    {
        // Arrange
        OrderPupilsHandler sut = new();

        List<Pupil> pupils = PupilTestDoubles.Generate(count: 10);

        OrderOptions options = new(field: field!, string.Empty);

        // Act
        IEnumerable<Pupil> result = sut.Order(pupils, options);

        // Assert
        Assert.Equal(pupils, result);
    }

    [Fact]
    public void Order_Returns_SameOrder_When_Options_Is_Null()
    {
        // Arrange
        OrderPupilsHandler sut = new();
        List<Pupil> pupils = PupilTestDoubles.Generate(count: 3);

        // Act
        IEnumerable<Pupil> result = sut.Order(pupils, null!);

        // Assert
        Assert.Equal(pupils, result);
    }

    [Fact]
    public void Order_Throws_When_Field_Is_Unknown()
    {
        // Arrange
        OrderPupilsHandler sut = new();

        OrderOptions options = new("unknown-sortByKey", string.Empty);

        // Act
        Action act = () => sut.Order(
            PupilTestDoubles.Generate(count: 5), options);

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Theory]
    [InlineData("forename", "asc")]
    [InlineData("forename", "desc")]
    [InlineData("FORENAME", "asc")]
    [InlineData("foRENamE", "desc")]
    public void Order_Sorts_By_Forename(string field, string direction)
    {
        // Arrange
        OrderPupilsHandler sut = new();
        List<Pupil> pupils = PupilTestDoubles.Generate(count: 20);
        OrderOptions options = new(field, direction);

        // Act
        IEnumerable<Pupil> result = sut.Order(pupils, options);

        // Assert
        IOrderedEnumerable<Pupil> expected =
            direction == "asc"
                ? pupils.OrderBy(t => t.Forename)
                : pupils.OrderByDescending(t => t.Forename);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("surname", "asc")]
    [InlineData("surname", "desc")]
    [InlineData("SURNAME", "asc")]
    [InlineData("suRnAme", "desc")]
    public void Order_Sorts_By_Surname(string field, string direction)
    {
        // Arrange
        OrderPupilsHandler sut = new();
        List<Pupil> pupils = PupilTestDoubles.Generate(count: 20);
        OrderOptions options = new(field, direction);

        // Act
        IEnumerable<Pupil> result = sut.Order(pupils, options);

        // Assert
        IOrderedEnumerable<Pupil> expected =
            direction == "asc"
                ? pupils.OrderBy(t => t.Surname)
                : pupils.OrderByDescending(t => t.Surname);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("dob", "asc")]
    [InlineData("dob", "desc")]
    [InlineData("DOB", "asc")]
    [InlineData("dOB", "desc")]
    public void Order_Sorts_By_DateOfBirth(string field, string direction)
    {
        // Arrange
        OrderPupilsHandler sut = new();

        List<Pupil> pupils = PupilTestDoubles.Generate(count: 20);

        OrderOptions options = new(field, direction);

        // Act
        IEnumerable<Pupil> result = sut.Order(pupils, options);

        // Assert
        IOrderedEnumerable<Pupil> expected =
            direction == "asc"
                ? pupils.OrderBy(t => t.TryParseDateOfBirth() ?? DateTime.MinValue)
                : pupils.OrderByDescending(t => t.TryParseDateOfBirth() ?? DateTime.MinValue);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("sex", "asc")]
    [InlineData("sex", "desc")]
    [InlineData("SEX", "asc")]
    [InlineData("seX", "desc")]
    public void Order_Sorts_By_Sex(string field, string direction)
    {
        // Arrange
        OrderPupilsHandler sut = new();
        List<Pupil> pupils = PupilTestDoubles.Generate(count: 20);
        OrderOptions options = new(field, direction);

        // Act
        IEnumerable<Pupil> result = sut.Order(pupils, options);

        // Assert
        IOrderedEnumerable<Pupil> expected =
            direction == "asc"
                ? pupils.OrderBy(t => t.Sex)
                : pupils.OrderByDescending(t => t.Sex);

        Assert.Equal(expected, result);
    }


    [Theory]
    [InlineData("forename", null)]
    [InlineData("forename", "")]
    [InlineData("forename", "unknown")]
    public void Order_Defaults_To_Ascending_When_Direction_Is_Missing_Or_Unknown(string field, string? direction)
    {
        // Arrange
        OrderPupilsHandler sut = new();
        List<Pupil> pupils = PupilTestDoubles.Generate(20);
        OrderOptions options = new(field, direction!);

        // Act
        IEnumerable<Pupil> result = sut.Order(pupils, options);

        // Assert
        Assert.Equal(pupils.OrderBy(t => t.Forename), result);
    }
}
