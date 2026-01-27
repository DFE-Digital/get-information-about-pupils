using DfE.GIAP.Core.Search.Application.Models.Learner;
using FluentAssertions;
using Model = DfE.GIAP.Core.Search.Application.Models.Learner;

namespace DfE.GIAP.Core.UnitTests.Search.Application.Models.Learner;

public sealed class LearnersTests
{
    [Fact]
    public void Constructor_WithValidLearnerList_ShouldInitializeCollection()
    {
        // arrange
        Model.FurtherEducationLearner learner =
            new(
                new FurtherEducationLearnerIdentifier("1234567890"),
                new LearnerName("Alice", "Smith"),
                new LearnerCharacteristics(
                    new DateTime(2005, 6, 1),
                    LearnerCharacteristics.Gender.Female)
            );

        List<Model.FurtherEducationLearner> learners = [learner];

        // act
        FurtherEducationLearners result = new(learners);

        // Assert
        result.Count.Should().Be(1);
        result.LearnerCollection.Should().ContainSingle()
            .Which.Should().Be(learner);
    }

    [Fact]
    public void Constructor_WithNullInput_ShouldInitializeEmptyCollection()
    {
        // act
        FurtherEducationLearners result = new(null!);

        // Assert
        result.Count.Should().Be(0);
        result.LearnerCollection.Should().BeEmpty();
    }

    [Fact]
    public void DefaultConstructor_ShouldInitializeEmptyCollection()
    {
        // act
        FurtherEducationLearners result = new();

        // Assert
        result.Count.Should().Be(0);
        result.LearnerCollection.Should().BeEmpty();
    }

    [Fact]
    public void CreateEmpty_ShouldReturnEmptyLearnersInstance()
    {
        // act
        FurtherEducationLearners result = FurtherEducationLearners.CreateEmpty();

        // Assert
        result.Count.Should().Be(0);
        result.LearnerCollection.Should().BeEmpty();
    }

    [Fact]
    public void LearnerCollection_ShouldBeReadOnly()
    {
        // arrange
        Model.FurtherEducationLearner learner = new(
            new FurtherEducationLearnerIdentifier("1234567890"),
            new LearnerName("Bob", "Jones"),
            new LearnerCharacteristics(
                new DateTime(2004, 3, 15),
                LearnerCharacteristics.Gender.Male)
        );

        FurtherEducationLearners result = new([learner]);

        // act
        Action mutate = () =>
            ((List<Model.FurtherEducationLearner>)result.LearnerCollection).Add(learner);

        // Assert
        mutate.Should().Throw<InvalidCastException>(); // AsReadOnly returns ReadOnlyCollection, not List
    }
}

