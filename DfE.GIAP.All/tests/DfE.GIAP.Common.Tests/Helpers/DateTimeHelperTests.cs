using DfE.GIAP.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xunit;

namespace DfE.GIAP.Common.Tests.Helpers
{
    public class DateTimeHelperTests
    {
        DateTime testDataAfter9 = DateTime.Parse("01/10/2021", new CultureInfo("en-GB"));

        [Fact]
        public void ConvertDateTimeToString_returns_correctly()
        {
            Assert.Equal("01/10/2021", DateTimeHelper.ConvertDateTimeToString(testDataAfter9));
        }

        [Fact]
        public void ConvertDateTimeToString_returns_null_when_date_is_null()
        {
            Assert.Null(DateTimeHelper.ConvertDateTimeToString(null));
        }
    }
}
