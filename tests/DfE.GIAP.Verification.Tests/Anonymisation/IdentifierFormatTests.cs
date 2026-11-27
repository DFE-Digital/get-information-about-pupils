using System.Numerics;
using DfE.GIAP.Verification.Anonymisation;

namespace DfE.GIAP.Verification.Tests.Anonymisation;

public class IdentifierFormatTests
{
    [Fact]
    public void Parse_ClassesRangesAndRepeats_GivesAnAlphabetForEachPosition()
    {
        IdentifierFormat format = IdentifierFormat.Parse("[G-IK]X[0-9]{3}");

        Assert.Equal(["GHIK", "X", "0123456789", "0123456789", "0123456789"], format.Alphabets);
        Assert.Equal(new BigInteger(4000), format.Size);
    }

    [Theory]
    [InlineData("")]
    [InlineData("[A-Z")]
    [InlineData("[Z-A]")]
    [InlineData("[0-9]{0}")]
    [InlineData("[0-9]{x}")]
    public void Parse_AnInvalidPattern_Throws(string pattern) =>
        Assert.Throws<FormatException>(() => IdentifierFormat.Parse(pattern));

    [Fact]
    public void ToNumberAndFromNumber_RoundTrip()
    {
        IdentifierFormat format = IdentifierFormat.Parse("[0-9A-F]{18}");

        Assert.Equal("CCA656CA36DABEFFC9", format.FromNumber(format.ToNumber("CCA656CA36DABEFFC9")));
        Assert.Equal(BigInteger.Zero, format.ToNumber("000000000000000000"));
        Assert.Equal(format.Size - 1, format.ToNumber("FFFFFFFFFFFFFFFFFF"));
    }

    [Theory]
    [InlineData("CCA656CA36DABEFFC9", true)]
    [InlineData("cca656ca36dabeffc9", false)]
    [InlineData("CCA656CA36DABEFFC", false)]
    [InlineData("GCA656CA36DABEFFC9", false)]
    public void Matches_ChecksEveryPosition(string value, bool expected) =>
        Assert.Equal(expected, IdentifierFormat.Parse("[0-9A-F]{18}").Matches(value));

    [Theory]
    [InlineData("[0-9A-F]{18}", "[G-V][0-9A-F]{17}", true)]
    [InlineData("[0-9A-F]{18}", "[A-Z][0-9A-F]{17}", false)]
    [InlineData("[A-Z][0-9]{12}", "[A-Z]{2}[0-9]{11}", true)]
    [InlineData("[A-Z][0-9]{12}", "[A-Z][0-9]{11}", true)]
    public void IsDisjointFrom_IsTrueOnlyIfNoValueMatchesBoth(string first, string second, bool expected) =>
        Assert.Equal(expected, IdentifierFormat.Parse(first).IsDisjointFrom(IdentifierFormat.Parse(second)));
}
