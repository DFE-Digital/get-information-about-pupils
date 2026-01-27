using DfE.GIAP.Core.Search.Application.Models.Learner;
using DfE.GIAP.Core.Search.Application.Models.Learner.FurtherEducation;
using FluentAssertions;

namespace DfE.GIAP.Core.UnitTests.Search.Application.Models.Learner;

public sealed class LearnerTests
{
    [Fact]
    public void Constructor_WithValidArguments_ShouldInitializeProperties()
    {
        // arrange
        FurtherEducationLearnerIdentifier identifier = new("1234567890");
        LearnerName name = new(firstName: "Alice", surname: "Smith");
        LearnerCharacteristics characteristics =
            new(
                DateTimeOffset.Parse("2005-06-01").Date,
                LearnerCharacteristics.Gender.Female
            );

        // act
        FurtherEducationLearner learner = new(identifier, name, characteristics);

        // Assert
        learner.Identifier.Should().Be(identifier);
        learner.Name.Should().Be(name);
        learner.Characteristics.Should().Be(characteristics);
    }

    [Fact]
    public void Constructor_WithNullName_ShouldThrowArgumentNullException()
    {
        // arrange
        FurtherEducationLearnerIdentifier identifier = new("1234567890");
        LearnerName? name = null;
        LearnerCharacteristics characteristics =
            new(
                DateTimeOffset.Now.Date,
                LearnerCharacteristics.Gender.Female
            );

        // act
        Action act = () =>
            new FurtherEducationLearner(identifier, name!, characteristics);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("name");
    }

    [Fact]
    public void Constructor_WithNullCharacteristics_ShouldThrowArgumentNullException()
    {
        // arrange
        FurtherEducationLearnerIdentifier identifier = new("1234567890");
        LearnerName name = new("Bob", "Jones");
        LearnerCharacteristics? characteristics = null;

        // act
        Action act = () =>
            new FurtherEducationLearner(identifier, name, characteristics!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("learnerCharacteristics");
    }
}

