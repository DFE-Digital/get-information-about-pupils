using System.Globalization;
using DfE.GIAP.Web.Features.Search.LegacyModels;
using DfE.GIAP.Web.Helpers;
using Xunit;

namespace DfE.GIAP.Common.Tests.Helpers;

public class RbacHelperTests
{
    [Fact]
    public void Encode_round_trip_works_correctly()
    {
        string value = "This should be equal by the time its gone around the encode/decode..";

        string encoded = RbacHelper.EncodeUpn(value);
        string decoded = RbacHelper.DecodeUpn(encoded);

        Assert.Equal(value, decoded);
    }

    [Theory]
    [InlineData("")]
    public void DecodeUpn_ReturnsEmptyWhenPassedEmpty(string value)
    {
        Assert.Equal(RbacHelper.DecodeUpn(value), string.Empty);
    }

    [Theory]
    [InlineData("01/10/2010", "01/10/2022", 12)]
    [InlineData("01/09/2010", "01/10/2022", 12)]
    [InlineData("01/11/2010", "01/10/2022", 11)]
    public void Calculate_age_works_correctly(string strDob, string strToday, int age)
    {
        DateTime dob = DateTime.Parse(strDob, new CultureInfo("en-gb"));
        DateTime today = DateTime.Parse(strToday, new CultureInfo("en-gb"));

        Assert.Equal(age, RbacHelper.CalculateAge(dob, today));
    }

    [Fact]
    public void CheckRbacRulesGeneric_returns_the_list_unchanged_if_the_rules_dont_apply()
    {
        // Arrange
        List<TestRbac> testData = GetTestList();

        // Act
        List<TestRbac> results = RbacHelper.CheckRbacRulesGeneric<TestRbac>(testData, 0, 0);

        // Assert
        foreach (TestRbac result in results)
        {
            Assert.NotEqual("*************", result.LearnerNumber);
        }
    }

    [Fact]
    public void CheckRbacRulesGeneric_returns_the_list_with_correct_pupils_starred_out()
    {
        // Arrange
        List<TestRbac> testData = GetTestList();

        // Act
        List<TestRbac> results = RbacHelper.CheckRbacRulesGeneric<TestRbac>(testData, 3, 11, DateTime.Parse("01/10/2022", new CultureInfo("en-gb")));

        // Assert
        Assert.Equal("*************", results[0].LearnerNumber);
        Assert.Equal("*************", results[1].LearnerNumber);
        Assert.Equal("*************", results[3].LearnerNumber);
    }

    private static List<TestRbac> GetTestList()
    {
        CultureInfo cultureInfo = new CultureInfo("en-gb");
        return
        [
            new()
            {
                DOB = DateTime.Parse("01/10/2010", cultureInfo),
                LearnerNumber = "123456789",
                LearnerNumberId = "123456789"

            },
            new()
            {
                DOB = DateTime.Parse("01/09/2010", cultureInfo),
                LearnerNumber = "912345678",
                LearnerNumberId = "912345678"

            },
            new()
            {
                DOB = DateTime.Parse("01/11/2010", cultureInfo),
                LearnerNumber = "891234567",
                LearnerNumberId = "891234567"

            },
            new()
            {
                LearnerNumber = "789123456",
                LearnerNumberId = "789123456"

            }
        ];
    }

    private class TestRbac : IRbac
    {
        public DateTime? DOB { get; set; }
        public string LearnerNumber { get; set; }
        public string LearnerNumberId { get; set; }
    }
}
