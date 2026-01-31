using DfE.GIAP.Core.Common.Application.ValueObjects;

namespace DfE.GIAP.Core.UnitTests.Common.Application.ValueObjects;
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
