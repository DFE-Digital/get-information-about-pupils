using DfE.GIAP.Verification.Anonymisation;

namespace DfE.GIAP.Verification.Tests.Anonymisation;

public class CheckCharactersTests
{
    [Theory]
    // 1×10 + 2×9 + … + 9×2 = 210, which is 1 more than a multiple of 11, so the check digit is 10 - 1
    [InlineData("1234567899", true)]
    [InlineData("1234567898", false)]
    [InlineData("0234567899", false)]
    [InlineData("123456789", false)]
    [InlineData("123456789X", false)]
    public void IsValidUln_ChecksTheCheckDigit(string uln, bool expected) =>
        Assert.Equal(expected, CheckCharacters.IsValidUln(uln));

    [Fact]
    public void UlnCheckDigit_IsNull_WhenTheRemainderIsZero() =>
        // 2×10 + 1×2 = 22, a multiple of 11
        Assert.Null(CheckCharacters.UlnCheckDigit("200000001"));

    [Theory]
    // The last digit is weighted 13, so the check letter is the 14th: P
    [InlineData("P000000000001", true)]
    [InlineData("A000000000001", false)]
    // A temporary UPN's last letter counts as its index: B is 1, so 1 × 13 = 13 again
    [InlineData("P00000000000B", true)]
    [InlineData("P00000000000I", false)]
    [InlineData("P00000000001", false)]
    public void IsValidUpn_ChecksTheCheckLetter(string upn, bool expected) =>
        Assert.Equal(expected, CheckCharacters.IsValidUpn(upn));
}
