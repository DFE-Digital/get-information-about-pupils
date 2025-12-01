using System.Reflection;
using DfE.GIAP.Core.Search.Application.Models.Learner;
using DfE.GIAP.Web.Controllers.TextBasedSearch.Mappers;
using DfE.GIAP.Web.Tests.Controllers.Search.TextBasedSearch.Mappers.TestDoubles;
using Xunit;
using static DfE.GIAP.Core.Search.Application.Models.Learner.LearnerCharacteristics;

namespace DfE.GIAP.Web.Tests.Controllers.Search.TextBasedSearch.Mappers;

public sealed class LearnerToViewModelMapperTests
{
    private readonly LearnerToViewModelMapper _mapper = new();

    [Fact]
    public void Map_WithNullInput_ThrowsArgumentNullException()
    {
        // act/assert
        Assert.Throws<ArgumentNullException>(() => _mapper.Map(null!));
    }

    [Fact]
    public void Map_WithValidLearner_MapsAllFieldsCorrectly()
    {
        // arrange
        Learner applicationModellearner =
            ApplicationModelLearnerTestDouble.Stub(
                uniqueLearnerNumber: "1234567890",
                firstname: "Alice",
                surname: "Smith",
                birthDate: new DateTime(2000, 1, 15),
                sex: LearnerCharacteristics.Gender.Female);

        // act
        Domain.Search.Learner.Learner result = _mapper.Map(applicationModellearner);

        // assert
        Assert.Equal("1234567890", result.Id);
        Assert.Equal("1234567890", result.LearnerNumber);
        Assert.Equal("Alice", result.Forename);
        Assert.Equal("Smith", result.Surname);
        Assert.Equal("F", result.Sex);
        Assert.Equal(new DateTime(2000, 1, 15), result.DOB);
    }

    [Theory]
    [InlineData(Gender.Male, "M")]
    [InlineData(Gender.Female, "F")]
    [InlineData(Gender.Other, "Unspecified")]
    public void MapGender_AssignsCorrectValue(Gender gender, string expected)
    {
        // act
        object? result =
            typeof(LearnerToViewModelMapper)
                .GetMethod("MapSexDescription", BindingFlags.NonPublic | BindingFlags.Static)!
                .Invoke(null, [gender]);

        // assert
        Assert.Equal(expected, result);
    }
}

