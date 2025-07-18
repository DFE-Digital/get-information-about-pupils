using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.UnitTests.MyPupils.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.MyPupils.Domain.ValueObjects;


public sealed class DateOfBirthTests
{
    [Fact]
    public void Constructor_SetsValue_WhenDateIsValid()
    {
        // Arrange
        DateTime inputDate = DateTimeTestDoubles.GenerateDateOfBirthForAgeOf(5);

        // Act
        DateOfBirth dob = new(inputDate);

        // Assert
        Assert.Equal(inputDate.ToString("yyyy-MM-dd"), dob.ToString());
    }

    [Fact]
    public void Constructor_ThrowsArgumentException_WhenDateIsInFuture()
    {
        // Arrange
        DateTime futureDate = DateTime.UtcNow.AddDays(1);

        // Act Assert
        ArgumentException ex = Assert.Throws<ArgumentException>(() => new DateOfBirth(futureDate));
        Assert.Equal("Date of birth cannot be in the future.", ex.Message);
    }

    [Fact]
    public void Age_ReturnsCorrectValue_WhenBirthdayHasAlreadyOccurredThisYear()
    {
        // Arrange
        DateTime dobDate = DateTimeTestDoubles.GenerateDateOfBirthThatHasAlreadyOccuredThisYear();
        int expectedAge = DateTime.UtcNow.Year - dobDate.Year;

        DateOfBirth dob = new(dobDate);

        // Act
        int age = dob.Age;

        // Assert
        Assert.Equal(expectedAge, age);
    }

    [Fact]
    public void Age_ReturnsCorrectValue_WhenBirthdayHasNotOccurredYetThisYear()
    {
        // Arrange
        DateTime dobDate = DateTimeTestDoubles.GenerateDateOfBirthThatHasNotOccuredYetThisYear();
        int expectedAge = DateTime.UtcNow.Year - dobDate.Year - 1;

        DateOfBirth dob = new(dobDate);

        // Act
        int age = dob.Age;

        // Assert
        Assert.Equal(expectedAge, age);
    }


    [Fact]
    public void ImplicitConversion_ReturnsDateTimeValue()
    {
        // Arrange
        DateTime date = DateTimeTestDoubles.GenerateFor(2010, 5, 20);
        DateOfBirth dob = new(date);

        // Act
        DateTime? result = dob;

        // Assert
        Assert.Equal(date, result);
    }

    [Fact]
    public void Equality_ReturnsTrue_ForSameDate()
    {
        // Arrange
        DateTime date = DateTimeTestDoubles.GenerateFor(2000, 1, 1);
        DateOfBirth dob1 = new(date);
        DateOfBirth dob2 = new(date);

        // Act Assert
        Assert.Equal(dob1, dob2);
        Assert.True(dob1.Equals(dob2));
        Assert.Equal(dob1.GetHashCode(), dob2.GetHashCode());
    }

    [Fact]
    public void Equality_ReturnsFalse_ForDifferentDates()
    {
        // Arrange
        DateOfBirth dob1 = new(DateTimeTestDoubles.GenerateFor(2000, 1, 1));
        DateOfBirth dob2 = new(DateTimeTestDoubles.GenerateFor(2001, 1, 1));

        // Act Assert
        Assert.NotEqual(dob1, dob2);
        Assert.False(dob1.Equals(dob2));
    }
}
