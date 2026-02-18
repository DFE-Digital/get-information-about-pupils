using System.ComponentModel;
using DfE.GIAP.Web.Helpers;
using Xunit;

namespace DfE.GIAP.Common.Tests.Helpers;

public sealed class StringHelperTests
{
    [Fact]
    public void FormatUpns_returns_null_if_empty_string_given()
    {
        Assert.Null("".FormatLearnerNumbers());
    }

    [Fact]
    public void FormatUpns_returns_array_if_upns_exist()
    {
        string upnString = "123\r\n234\r\n345 \r";
        string[] upnArray = upnString.FormatLearnerNumbers();
        Assert.Equal("123", upnArray[0]);
        Assert.Equal("234", upnArray[1]);
        Assert.Equal("345", upnArray[2]);
    }

    [Theory]
    [InlineData(TestEnum.TestItem, "different description")]
    [InlineData(TestEnum.ToStringItem, "ToStringItem")]
    public void StringValueOfEnum_returns_correct_value(Enum test, string expected)
    {
        Assert.Equal(expected, StringHelper.StringValueOfEnum(test));
    }

    private enum TestEnum
    {
        [Description("different description")]
        TestItem,
        ToStringItem
    }
}
