using DfE.GIAP.Core.Common.Application.ValueObjects;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.Models;
using FluentAssertions;

namespace DfE.GIAP.Core.UnitTests.Search.Application.UseCases.FurtherEducation;

public sealed class FurtherEducationLearnersTests
{
    [Fact]
    public void Constructor_WithValidLearnerList_ShouldInitializeCollection()
    {
        // arrange
        FurtherEducationLearner learner =
            new(
                new FurtherEducationUniqueLearnerIdentifier("1234567890"),
                new LearnerName("Alice", "Smith"),
                new LearnerCharacteristics(
                    new DateTime(2005, 6, 1),
                    Sex.Female)
            );

        List<FurtherEducationLearner> learners = [learner];

        // act
        FurtherEducationLearners result = new(learners);

        // Assert
        result.Count.Should().Be(1);
        result.Learners.Should().ContainSingle()
            .Which.Should().Be(learner);
    }

    [Fact]
    public void Constructor_WithNullInput_ShouldInitializeEmptyCollection()
    {
        // act
        FurtherEducationLearners result = new(null!);

        // Assert
        result.Count.Should().Be(0);
        result.Learners.Should().BeEmpty();
    }

    [Fact]
    public void DefaultConstructor_ShouldInitializeEmptyCollection()
    {
        // act
        FurtherEducationLearners result = new();

        // Assert
        result.Count.Should().Be(0);
        result.Learners.Should().BeEmpty();
    }

    [Fact]
    public void CreateEmpty_ShouldReturnEmptyLearnersInstance()
    {
        // act
        FurtherEducationLearners result = FurtherEducationLearners.CreateEmpty();

        // Assert
        result.Count.Should().Be(0);
        result.Learners.Should().BeEmpty();
    }

    [Fact]
    public void LearnerCollection_ShouldBeReadOnly()
    {
        // arrange
        FurtherEducationLearner learner = new(
            new FurtherEducationUniqueLearnerIdentifier("1234567890"),
            new LearnerName("Bob", "Jones"),
            new LearnerCharacteristics(
                new DateTime(2004, 3, 15),
                Sex.Male)
        );

        FurtherEducationLearners result = new([learner]);

        // act
        Action mutate = () =>
            ((List<FurtherEducationLearner>)result.Learners).Add(learner);

        // Assert
        mutate.Should().Throw<InvalidCastException>(); // AsReadOnly returns ReadOnlyCollection, not List
    }
}

