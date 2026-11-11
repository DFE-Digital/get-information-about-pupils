using DfE.GIAP.Core.Common.Application.Helpers;

namespace DfE.GIAP.Core.UnitTests.Common.Application.Helpers;

public class DateFormattingTests
{
    [Fact]
    public void ToStandardDate_FormatsDate_AsDdMmYyyy()
    {
        DateTime date = new DateTime(2000, 1, 1);

        string? result = DateFormatting.ToStandardDate(date);

        Assert.Equal("01/01/2000", result);
    }

    [Fact]
    public void ToStandardDate_UsesInvariantCulture()
    {
        DateTime date = new DateTime(2024, 12, 5);

        string? result = DateFormatting.ToStandardDate(date);

        Assert.Equal("05/12/2024", result);
    }

    [Fact]
    public void ToStandardDate_ReturnsNull_WhenDateIsNull()
    {
        string? result = DateFormatting.ToStandardDate(null);

        Assert.Null(result);
    }
}

