using DfE.GIAP.Core.Search.Application.Models.Learner;
using FluentAssertions;
using Model = DfE.GIAP.Core.Search.Application.Models.Learner;

namespace DfE.GIAP.Core.UnitTests.Search.Application.Models.Learner;

public sealed class LearnerTests
{
    [Fact]
    public void Constructor_WithValidArguments_ShouldInitializeProperties()
    {
        // Arrange
        LearnerIdentifier identifier = new("1234567890");
        LearnerName name = new(firstName: "Alice", surname: "Smith");
        LearnerCharacteristics characteristics =
            new(
                DateTimeOffset.Parse("2005-06-01").Date,
                LearnerCharacteristics.Gender.Female
            );

        // Act
        Model.Learner learner = new(identifier, name, characteristics);

        // Assert
        learner.Identifier.Should().Be(identifier);
        learner.Name.Should().Be(name);
        learner.Characteristics.Should().Be(characteristics);
    }

    [Fact]
    public void Constructor_WithNullName_ShouldThrowArgumentNullException()
    {
        // Arrange
        LearnerIdentifier identifier = new("1234567890");
        LearnerName? name = null;
        LearnerCharacteristics characteristics =
            new(
                DateTimeOffset.Now.Date,
                LearnerCharacteristics.Gender.Female
            );

        // Act
        Action act = () =>
            new Model.Learner(identifier, name!, characteristics);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("name");
    }

    [Fact]
    public void Constructor_WithNullCharacteristics_ShouldThrowArgumentNullException()
    {
        // Arrange
        LearnerIdentifier identifier = new("1234567890");
        LearnerName name = new("Bob", "Jones");
        LearnerCharacteristics? characteristics = null;

        // Act
        Action act = () =>
            new Model.Learner(identifier, name, characteristics!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("learnerCharacteristics");
    }
}

