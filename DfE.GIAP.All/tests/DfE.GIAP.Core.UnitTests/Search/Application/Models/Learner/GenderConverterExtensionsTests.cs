using DfE.GIAP.Core.Search.Application.Models.Learner;

namespace DfE.GIAP.Core.UnitTests.Search.Application.Models.Learner;
public sealed class GenderConverterExtensionsTests
{
    [Theory]
    [InlineData(Gender.Male, "M")]
    [InlineData(Gender.Female, "F")]
    [InlineData(Gender.Other, "Unspecified")]
    public void MapGender_AssignsCorrectValue(Gender gender, string expected)
    {
        // act
        string result = gender.MapSexDescription();

        // assert
        Assert.Equal(expected, result);
    }
}
