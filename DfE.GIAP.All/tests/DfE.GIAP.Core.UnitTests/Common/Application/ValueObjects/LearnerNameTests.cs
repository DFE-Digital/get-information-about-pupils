using DfE.GIAP.Core.Common.Application.ValueObjects;

namespace DfE.GIAP.Core.UnitTests.Common.Application.ValueObjects;

public sealed class LearnerNameTests
{
    [Fact]
    public void Constructor_WithValidFirstLastNames_ShouldInitializeProperties()
    {
        // arrange
        const string firstName = "Alice";
        const string surname = "Smith";

        // act
        LearnerName learnerName = new(firstName, surname);

        // Assert
        Assert.Equal(firstName, learnerName.FirstName);
        Assert.Equal(surname, learnerName.Surname);
    }

    [Fact]
    public void Constructor_WithValidFirstMiddleLastNames_ShouldInitializeProperties()
    {
        // arrange
        const string firstName = "Alice";
        const string middleName = "Beth";
        const string surname = "Smith";

        // act
        LearnerName learnerName = new(firstName, middleName, surname);

        // Assert
        Assert.Equal(firstName, learnerName.FirstName);
        Assert.Equal(middleName, learnerName.MiddleNames);
        Assert.Equal(surname, learnerName.Surname);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\n")]
    public void Constructor_WithValidNames_AndEmptyOrNullMiddleName_Should_NormaliseMiddleName(string? middleNameInput)
    {
        // act
        LearnerName learnerName = new(
            firstName: "John",
            middleName: middleNameInput,
            surname: "Doe");

        // Assert
        Assert.Equal("John", learnerName.FirstName);
        Assert.Equal(string.Empty, learnerName.MiddleNames);
        Assert.Equal("Doe", learnerName.Surname);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\n")]
    public void Constructor_WithInvalidFirstName_ShouldThrowArgumentException(string? invalidFirstName)
    {
        // act
        Func<LearnerName> act = () =>
            new(invalidFirstName!, "ValidSurname");

        // Assert
        Assert.ThrowsAny<ArgumentException>(act);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\n")]
    public void Constructor_WithInvalidSurname_ShouldThrowArgumentException(string? invalidSurname)
    {
        // act
        Func<LearnerName> act = () =>
            new("ValidForename"!, invalidSurname!);

        // Assert
        Assert.ThrowsAny<ArgumentException>(act);
    }


    [Theory]
    [InlineData("anne-marie", "Anne-Marie")]
    [InlineData("Anne-marie", "Anne-Marie")]
    [InlineData("ANNE-MARIE", "Anne-Marie")]
    [InlineData("jean-luc", "Jean-Luc")]
    [InlineData("jean-luc-picard", "Jean-Luc-Picard")]
    [InlineData("  mary-jane-smith  ", "Mary-Jane-Smith")]
    public void Constructor_WithHyphenatedSurname_ShouldNormaliseBarrels(string inputSurname, string expectedSurname)
    {
        // arrange
        const string firstName = "Alice";

        // act
        LearnerName learnerName = new LearnerName(firstName, inputSurname);

        // assert
        Assert.Equal("Alice", learnerName.FirstName);
        Assert.Equal(expectedSurname, learnerName.Surname);
    }

    [Theory]
    [InlineData("anne-marie", "Anne-Marie")]
    [InlineData("JEAN-LUC", "Jean-Luc")]
    [InlineData("jean-luc-picard", "Jean-Luc-Picard")]
    public void Constructor_WithHyphenatedFirstName_ShouldNormaliseBarrels(string inputFirstName, string expectedFirstName)
    {
        // arrange
        const string surname = "Smith";

        // act
        LearnerName learnerName = new LearnerName(inputFirstName, surname);

        // assert
        Assert.Equal(expectedFirstName, learnerName.FirstName);
        Assert.Equal("Smith", learnerName.Surname);
    }

    [Theory]
    [InlineData("d'amore", "D'Amore")]
    [InlineData("D'aMoRe", "D'Amore")]
    [InlineData("O'NEILL", "O'Neill")]
    [InlineData("o'bRIeN", "O'Brien")]
    [InlineData("  d'arcy  ", "D'Arcy")]
    public void Constructor_WithApostrophisedSurname_ShouldNormaliseSegments(string inputSurname, string expectedSurname)
    {
        // arrange
        const string firstName = "John";

        // act
        LearnerName learnerName = new LearnerName(firstName, inputSurname);

        // assert
        Assert.Equal("John", learnerName.FirstName);
        Assert.Equal(expectedSurname, learnerName.Surname);
    }

    [Theory]
    [InlineData("o'neill", "O'Neill")]
    [InlineData("O'bRIeN", "O'Brien")]
    public void Constructor_WithApostrophisedFirstName_ShouldNormaliseSegments(string inputFirstName, string expectedFirstName)
    {
        // arrange
        const string surname = "Doe";

        // act
        LearnerName learnerName = new(inputFirstName, surname);

        // assert
        Assert.Equal(expectedFirstName, learnerName.FirstName);
        Assert.Equal("Doe", learnerName.Surname);
    }

    [Theory]
    [InlineData("d'amore-smith", "D'Amore-Smith")]
    [InlineData("o'neill-jones", "O'Neill-Jones")]
    [InlineData("jean d'amore-smith", "Jean D'Amore-Smith")]
    public void Constructor_WithApostrophesAndHyphens_ShouldNormaliseEachSegment(string inputSurname, string expectedSurname)
    {
        // arrange
        const string firstName = "Jean";

        // act
        LearnerName learnerName = new(firstName, inputSurname);

        // assert
        Assert.Equal("Jean", learnerName.FirstName);
        Assert.Equal(expectedSurname, learnerName.Surname);
    }

    [Theory]
    [InlineData(" d'amore ", "D'Amore")]
    [InlineData("mary-jane", "Mary-Jane")]
    [InlineData("O'NEILL", "O'Neill")]
    public void Constructor_WithDelimitedMiddleNames_ShouldNormalise(string middleInput, string expectedMiddle)
    {
        // arrange
        const string firstName = "Alice";
        const string surname = "Smith";

        // act
        LearnerName learnerName = new(firstName, middleInput, surname);

        // assert
        Assert.Equal("Alice", learnerName.FirstName);
        Assert.Equal(expectedMiddle, learnerName.MiddleNames);
        Assert.Equal("Smith", learnerName.Surname);
    }


    [Fact]
    public void Equality_WithSameFirstLastNames_ShouldBeEqual()
    {
        // arrange
        LearnerName equalityCheckInstanceA = new("John", "Doe");
        LearnerName equalityCheckInstanceB = new("John", "Doe");

        // act & Assert
        Assert.Equal(equalityCheckInstanceA, equalityCheckInstanceB);
        Assert.True(equalityCheckInstanceA.Equals(equalityCheckInstanceB));
    }

    [Theory]
    [InlineData("john", "doe")]
    [InlineData("john", "Doe")]
    [InlineData("John", "doe")]
    public void Equality_WithSameNormalisedFirstLastValues_ShouldBeEqual(string firstName, string lastName)
    {
        // Arrange
        LearnerName inputName = new(firstName, lastName);
        LearnerName comparator = new("John", "Doe");

        // Act Assert
        Assert.Equal(inputName, comparator);
    }

    [Theory]
    [InlineData("john", "smith", "doe")]
    [InlineData("john", "Smith", "Doe")]
    [InlineData("John", "SMiTh", "doe")]
    public void Equality_WithSameNormalisedFirstMiddleLastValues_ShouldBeEqual(string firstName, string middleName, string lastName)
    {
        // Arrange
        LearnerName inputName = new(firstName, middleName, lastName);
        LearnerName comparator = new("John", "Smith", "Doe");

        // Act Assert
        Assert.Equal(inputName, comparator);
    }

    [Theory]
    [InlineData("jean-luc", "picard")]
    [InlineData("Jean-Luc", "Picard")]
    [InlineData("JEAN-LUC", "PICARD")]
    public void Equality_WithHyphenatedFirstName_Normalised_ShouldBeEqual(string firstName, string surname)
    {
        // arrange
        LearnerName input = new LearnerName(firstName, surname);
        LearnerName canonical = new LearnerName("Jean-Luc", "Picard");

        // act & assert
        Assert.Equal(canonical, input);
    }

    [Theory]
    [InlineData("o'neill")]
    [InlineData("O'NEILL")]
    [InlineData("o'NEiLl")]
    public void Equality_WithApostrophisedSurname_Normalised_ShouldBeEqual(string surname)
    {
        // arrange
        LearnerName input = new LearnerName("Shaun", surname);
        LearnerName canonical = new LearnerName("Shaun", "O'Neill");

        // act & assert
        Assert.Equal(canonical, input);
    }

    [Theory]
    [InlineData("anne-marie", "beth", "o'brien")]
    [InlineData("Anne-Marie", "BETH", "O'BRIEN")]
    [InlineData("ANNE-MARIE", "BeTh", "o'BRien")]
    public void Equality_WithHyphensAndApostrophes_InAllParts_ShouldBeEqual(string firstName, string middleNames, string surname)
    {
        // arrange
        LearnerName input = new LearnerName(firstName, middleNames, surname);
        LearnerName canonical = new LearnerName("Anne-Marie", "Beth", "O'Brien");

        // act & assert
        Assert.Equal(canonical, input);
    }

    [Fact]
    public void Equality_WithDifferentFirstNames_SameLastNames_ShouldNotBeEqual()
    {
        // arrange
        LearnerName equalityCheckInstanceA = new("Michael", "Doe");
        LearnerName equalityCheckInstanceB = new("Jane", "Doe");

        // act & Assert
        Assert.NotEqual(equalityCheckInstanceA, equalityCheckInstanceB);
        Assert.False(equalityCheckInstanceA.Equals(equalityCheckInstanceB));
    }

    [Fact]
    public void Equality_WithDifferentLastNames_SameFirstNames_ShouldNotBeEqual()
    {
        // arrange
        LearnerName equalityCheckInstanceA = new("John", "Doe");
        LearnerName equalityCheckInstanceB = new("Jane", "Donald");

        // act & Assert
        Assert.NotEqual(equalityCheckInstanceA, equalityCheckInstanceB);
        Assert.False(equalityCheckInstanceA.Equals(equalityCheckInstanceB));
    }

    [Fact]
    public void Equality_WithSameFirstMiddleLastNames_ShouldBeEqual()
    {
        // arrange
        LearnerName equalityCheckInstanceA = new("John", "Knight", "Doe");
        LearnerName equalityCheckInstanceB = new("John", "Knight", "Doe");

        // act & Assert
        Assert.Equal(equalityCheckInstanceA, equalityCheckInstanceB);
        Assert.True(equalityCheckInstanceA.Equals(equalityCheckInstanceB));
    }

    [Fact]
    public void Equality_WithDifferentFirstNames_SameMiddleLastNames_ShouldNotBeEqual()
    {
        // arrange
        LearnerName equalityCheckInstanceA = new("Michael", "Knight", "Doe");
        LearnerName equalityCheckInstanceB = new("Jane", "Knight", "Doe");

        // act & Assert
        Assert.NotEqual(equalityCheckInstanceA, equalityCheckInstanceB);
        Assert.False(equalityCheckInstanceA.Equals(equalityCheckInstanceB));
    }


    [Fact]
    public void Equality_WithDifferentMiddleNames_SameFirstLastNames_ShouldNotBeEqual()
    {
        // arrange
        LearnerName equalityCheckInstanceA = new("John", "Knight", "Doe");
        LearnerName equalityCheckInstanceB = new("Jane", "Lancelot", "Doe");

        // act & Assert
        Assert.NotEqual(equalityCheckInstanceA, equalityCheckInstanceB);
        Assert.False(equalityCheckInstanceA.Equals(equalityCheckInstanceB));
    }

    [Fact]
    public void Equality_WithDifferentLastNames_SameFirstMiddleNames_ShouldNotBeEqual()
    {
        // arrange
        LearnerName equalityCheckInstanceA = new("John", "Knight", "Doe");
        LearnerName equalityCheckInstanceB = new("Jane", "Knight", "Donald");

        // act & Assert
        Assert.NotEqual(equalityCheckInstanceA, equalityCheckInstanceB);
        Assert.False(equalityCheckInstanceA.Equals(equalityCheckInstanceB));
    }
}

