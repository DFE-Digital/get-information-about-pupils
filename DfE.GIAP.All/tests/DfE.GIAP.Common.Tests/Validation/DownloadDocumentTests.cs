using DfE.GIAP.Common.Enums;
using DfE.GIAP.Common.Validation;
using Xunit;

namespace DfE.GIAP.Common.Tests.Validation;

[Trait("Validator", "Download Document")]
public class DownloadDocumentTests
{
    [Theory]
    [InlineData(DownloadDataType.EYFSP, 2, 15)]
    [InlineData(DownloadDataType.EYFSP, 10, 17)]
    public void EarlyYearsFoundationStagePupil_Validation_Should_Pass(DownloadDataType type, int statutoryLowAge, int statutoryHighAge)
    {
        bool result = DownloadValidation.ValidateDownloadDataType(type, statutoryLowAge, statutoryHighAge);
        Assert.True(result);
    }

    [Theory]
    [InlineData(DownloadDataType.EYFSP, 11, 15)]
    [InlineData(DownloadDataType.EYFSP, 11, 34)]
    [InlineData(DownloadDataType.EYFSP, 11, 4)]
    public void EarlyYearsFoundationStagePupil_Validation_Should_Fail(DownloadDataType type, int statutoryLowAge, int statutoryHighAge)
    {
        bool result = DownloadValidation.ValidateDownloadDataType(type, statutoryLowAge, statutoryHighAge);
        Assert.False(result);
    }

    [Theory]
    [InlineData(DownloadDataType.KS1, 2, 6)]
    [InlineData(DownloadDataType.KS1, 12, 17)]
    public void KeyStageOne_Validation_Should_Pass(DownloadDataType type, int statutoryLowAge, int statutoryHighAge)
    {
        bool result = DownloadValidation.ValidateDownloadDataType(type, statutoryLowAge, statutoryHighAge);
        Assert.True(result);
    }

    [Theory]
    [InlineData(DownloadDataType.KS1, 1, 15)]
    [InlineData(DownloadDataType.KS1, 2, 4)]
    [InlineData(DownloadDataType.KS1, 5, 26)]
    [InlineData(DownloadDataType.KS1, 10, 7)]
    public void KeyStageOne_Validation_Should_Fail(DownloadDataType type, int statutoryLowAge, int statutoryHighAge)
    {
        bool result = DownloadValidation.ValidateDownloadDataType(type, statutoryLowAge, statutoryHighAge);
        Assert.False(result);
    }

    [Theory]
    [InlineData(DownloadDataType.KS2, 10, 16)]
    [InlineData(DownloadDataType.KS2, 12, 23)]
    public void KeyStageTwo_Validation_Should_Pass(DownloadDataType type, int statutoryLowAge, int statutoryHighAge)
    {
        bool result = DownloadValidation.ValidateDownloadDataType(type, statutoryLowAge, statutoryHighAge);
        Assert.True(result);
    }

    [Theory]
    [InlineData(DownloadDataType.KS2, 17, 25)]
    [InlineData(DownloadDataType.KS2, 2, 4)]
    [InlineData(DownloadDataType.KS2, 15, 26)]
    [InlineData(DownloadDataType.KS2, 16, 13)]
    public void KeyStageTwo_Validation_Should_Fail(DownloadDataType type, int statutoryLowAge, int statutoryHighAge)
    {
        bool result = DownloadValidation.ValidateDownloadDataType(type, statutoryLowAge, statutoryHighAge);
        Assert.False(result);
    }

    [Theory]
    [InlineData(DownloadDataType.KS4, 13, 16)]
    [InlineData(DownloadDataType.KS4, 2, 23)]
    public void KeyStageFour_Validation_Should_Pass(DownloadDataType type, int statutoryLowAge, int statutoryHighAge)
    {
        bool result = DownloadValidation.ValidateDownloadDataType(type, statutoryLowAge, statutoryHighAge);
        Assert.True(result);
    }

    [Theory]
    [InlineData(DownloadDataType.KS4, 18, 25)]
    [InlineData(DownloadDataType.KS4, 2, 4)]
    [InlineData(DownloadDataType.KS4, 15, 26)]
    [InlineData(DownloadDataType.KS4, 15, 12)]
    public void KeyStageFour_Validation_Should_Fail(DownloadDataType type, int statutoryLowAge, int statutoryHighAge)
    {
        bool result = DownloadValidation.ValidateDownloadDataType(type, statutoryLowAge, statutoryHighAge);
        Assert.False(result);
    }

    [Theory]
    [InlineData(DownloadDataType.Phonics, 3, 8)]
    [InlineData(DownloadDataType.Phonics, 7, 23)]
    public void Phonics_Validation_Should_Pass(DownloadDataType type, int statutoryLowAge, int statutoryHighAge)
    {
        bool result = DownloadValidation.ValidateDownloadDataType(type, statutoryLowAge, statutoryHighAge);
        Assert.True(result);
    }

    [Theory]
    [InlineData(DownloadDataType.Phonics, 18, 20)]
    [InlineData(DownloadDataType.Phonics, 1, 2)]
    [InlineData(DownloadDataType.Phonics, 4, 26)]
    [InlineData(DownloadDataType.Phonics, 15, 12)]
    public void Phonics_Validation_Should_Fail(DownloadDataType type, int statutoryLowAge, int statutoryHighAge)
    {
        bool result = DownloadValidation.ValidateDownloadDataType(type, statutoryLowAge, statutoryHighAge);
        Assert.False(result);
    }

    [Theory]
    [InlineData(DownloadDataType.Phonics, 1, 12)]
    [InlineData(DownloadDataType.Phonics, 10, 29)]
    [InlineData(DownloadDataType.Phonics, 12, 26)]
    [InlineData(DownloadDataType.Phonics, 15, 12)]
    public void Default_Validation_Should_Fail_Because_Out_Of_Range(DownloadDataType type, int statutoryLowAge, int statutoryHighAge)
    {
        bool result = DownloadValidation.ValidateDownloadDataType(type, statutoryLowAge, statutoryHighAge);
        Assert.False(result);
    }

    [Theory]
    [InlineData(DownloadUlnDataType.PP, 12)]
    [InlineData(DownloadUlnDataType.SEN, 12)]
    public void ValidateUlnDownloadDataType_Should_fail_because_of_out_of_range(DownloadUlnDataType type, int statutoryHighAge)
    {
        bool result = DownloadValidation.ValidateUlnDownloadDataType(type, statutoryHighAge);
        Assert.False(result);
    }

    [Theory]
    [InlineData(DownloadUlnDataType.PP, 15)]
    [InlineData(DownloadUlnDataType.SEN, 16)]
    public void ValidateUlnDownloadDataType_Should_pass_because_in_range(DownloadUlnDataType type, int statutoryHighAge)
    {
        bool result = DownloadValidation.ValidateUlnDownloadDataType(type, statutoryHighAge);
        Assert.True(result);
    }
}
