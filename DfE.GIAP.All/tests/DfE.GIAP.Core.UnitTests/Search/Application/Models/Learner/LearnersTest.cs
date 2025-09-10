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
        Model.Learner learner =
            new(
                new LearnerIdentifier("1234567890"),
                new LearnerName("Alice", "Smith"),
                new LearnerCharacteristics(
                    new DateTime(2005, 6, 1),
                    LearnerCharacteristics.Gender.Female)
            );

        List<Model.Learner> learners = [learner];

        // act
        Learners result = new(learners);

        // Assert
        result.Count.Should().Be(1);
        result.LearnerCollection.Should().ContainSingle()
            .Which.Should().Be(learner);
    }

    [Fact]
    public void Constructor_WithNullInput_ShouldInitializeEmptyCollection()
    {
        // act
        Learners result = new(null!);

        // Assert
        result.Count.Should().Be(0);
        result.LearnerCollection.Should().BeEmpty();
    }

    [Fact]
    public void DefaultConstructor_ShouldInitializeEmptyCollection()
    {
        // act
        Learners result = new();

        // Assert
        result.Count.Should().Be(0);
        result.LearnerCollection.Should().BeEmpty();
    }

    [Fact]
    public void CreateEmpty_ShouldReturnEmptyLearnersInstance()
    {
        // act
        Learners result = Learners.CreateEmpty();

        // Assert
        result.Count.Should().Be(0);
        result.LearnerCollection.Should().BeEmpty();
    }

    [Fact]
    public void LearnerCollection_ShouldBeReadOnly()
    {
        // arrange
        Model.Learner learner = new(
            new LearnerIdentifier("1234567890"),
            new LearnerName("Bob", "Jones"),
            new LearnerCharacteristics(
                new DateTime(2004, 3, 15),
                LearnerCharacteristics.Gender.Male)
        );

        Learners result = new([learner]);

        // act
        Action mutate = () =>
            ((List<Model.Learner>)result.LearnerCollection).Add(learner);

        // Assert
        mutate.Should().Throw<InvalidCastException>(); // AsReadOnly returns ReadOnlyCollection, not List
    }
}

