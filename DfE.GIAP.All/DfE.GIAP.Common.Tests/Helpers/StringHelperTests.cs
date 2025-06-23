using DfE.GIAP.Common.Enums;
using DfE.GIAP.Common.Helpers;
using DfE.GIAP.Common.Validation;
using System;
using System.ComponentModel;
using Xunit;

namespace DfE.GIAP.Common.Tests.Helpers
{
    public class StringHelperTests
    {
        [Fact]
        public void FormatUpns_returns_null_if_empty_string_given()
        {
            Assert.Null("".FormatLearnerNumbers());
        }

        [Fact]
        public void FormatUpns_returns_array_if_upns_exist()
        {
            var upnString = "123\r\n234\r\n345 \r";
            var upnArray = upnString.FormatLearnerNumbers();
            Assert.Equal("123", upnArray[0]);
            Assert.Equal("234", upnArray[1]);
            Assert.Equal("345", upnArray[2]);
        }

        [Theory]
        [InlineData("filename.txt", "filename.txt")]
        [InlineData("filename.txt", "long/path/with/filename.txt")]
        public void GetMetaDataFileName_returns_file_name(string expected, string test)
        {
            Assert.Equal(expected, StringHelper.GetMetaDataFileName(test));
        }

        [Theory]
        [InlineData("filename", "filename.txt")]
        public void GetMetaDataName_returns_download_name(string expected, string test)
        {
            Assert.Equal(expected, StringHelper.GetMetaDataName(test));
        }

        [Fact]
        public void EliminateSanitizeDefaultText_returns_empty_if_string_only_contains_nbsp()
        {
            Assert.Equal(string.Empty, "&nbsp;".EliminateSanitizeDefaultText());
        }
        
        [Fact]
        public void EliminateSanitizeDefaultText_returns_input_if_string_contains_text()
        {
            Assert.Equal("test", "test".EliminateSanitizeDefaultText());
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
}
