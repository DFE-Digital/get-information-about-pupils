using DfE.GIAP.Common.Constants;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Helpers.Search;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Xunit;

namespace DfE.GIAP.Web.Tests.Helpers;

public sealed class PupilHelperTests
{
    [Fact]
    public void CheckIfStarredPupil_enter_null_return_false()
    {
        // Arrange
        string? inputData = null;

        // Act
        bool acting = PupilHelper.CheckIfStarredPupil(inputData);

        // Assert
        Assert.False(acting);
    }

    [Fact]
    public void CheckIfStarredPupil_enter_empty_return_false()
    {
        // Arrange
        string inputData = string.Empty;

        // Act
        bool acting = PupilHelper.CheckIfStarredPupil(inputData);

        // Assert
        Assert.False(acting);
    }

    [Fact]
    public void CheckIfStarredPupil_enter_equals_return_true()
    {
        // Arrange
        string inputData = Global.EncodedSuffixMarker;

        // Act
        bool acting = PupilHelper.CheckIfStarredPupil(inputData);

        // Assert
        Assert.True(acting);
    }

    [Fact]
    public void CheckIfStarredPupil_Array_enter_empty_return_false()
    {
        // Arrange
        string[] inputData = [];

        // Act
        bool acting = PupilHelper.CheckIfStarredPupil(inputData);

        // Assert
        Assert.False(acting);
    }

    [Fact]
    public void CheckIfStarredPupil_Array_enter_null_return_false()
    {
        // Arrange
        string[]? inputData = null;

        // Act
        bool acting = PupilHelper.CheckIfStarredPupil(inputData);

        // Assert
        Assert.False(acting);
    }

    [Fact]
    public void CheckIfStarredPupil_Array_enter_equals_return_true()
    {
        // Arrange
        string[] inputData = [Global.EncodedSuffixMarker];

        // Act
        bool acting = PupilHelper.CheckIfStarredPupil(inputData);

        // Assert
        Assert.True(acting);
    }

    [Fact]
    public void CheckIfStarredPupil_Array_enter_sequence_equals_return_true()
    {
        // Arrange
        string[] inputData = [" ", "", string.Empty, "!!", Global.EncodedSuffixMarker];

        // Act
        bool acting = PupilHelper.CheckIfStarredPupil(inputData);

        // Assert
        Assert.True(acting);
    }

    [Theory]
    [InlineData("<span style='display:none'>1</span>", Messages.Search.Errors.EnterUPNs)]
    [InlineData("You have not entered any UPNs", Messages.Search.Errors.EnterUPNs)]
    [InlineData("<span style='display:none'>2</span>", Messages.Search.Errors.TooManyUPNs)]
    [InlineData("UPNs have been entered, please review and reduce to the maximum of", Messages.Search.Errors.TooManyUPNs)]
    [InlineData("<span style='display:none'>3</span>", Messages.Search.Errors.UPNLength)]
    [InlineData("<span style='display:none'>4</span>", Messages.Search.Errors.UPNFormat)]
    [InlineData("<span style='display:none'>5</span>", Messages.Search.Errors.UPNMustBeUnique)]
    [InlineData("The following UPN(s) are duplicated", Messages.Search.Errors.UPNMustBeUnique)]
    public void GenerateValidationMessageUpnSearch_CheckOutcome(string inputData, string outputValue)
    {
        // Arrange
        ModelStateDictionary modelState = new ModelStateDictionary();
        modelState.AddModelError("test", inputData);

        // Act
        string acting = PupilHelper.GenerateValidationMessageUpnSearch(modelState);

        // Assert
        Assert.Equal(outputValue, acting);
    }

    [Theory]
    [InlineData("<span style='display:none'>1</span>", Messages.Search.Errors.EnterULNs)]
    [InlineData("You have not entered any ULNs", Messages.Search.Errors.EnterULNs)]
    [InlineData("<span style='display:none'>2</span>", Messages.Search.Errors.TooManyULNs)]
    [InlineData("ULNs have been entered, please review and reduce to the maximum of", Messages.Search.Errors.TooManyULNs)]
    [InlineData("<span style='display:none'>3</span>", Messages.Search.Errors.ULNLength)]
    [InlineData("<span style='display:none'>4</span>", Messages.Search.Errors.ULNFormat)]

    public void GenerateValidationMessageUlnSearch_CheckOutcome(string inputData, string outputValue)
    {
        // Arrange
        ModelStateDictionary modelState = new ModelStateDictionary();
        modelState.AddModelError("test", inputData);

        // Act
        string acting = PupilHelper.GenerateValidationMessageUlnSearch(modelState);

        // Assert
        Assert.Equal(outputValue, acting);
    }

    [Theory]
    [InlineData("31/12/2002", 31, 12, 2002)]
    [InlineData("2002", 0, 0, 2002)]
    [InlineData("12/2002", 0, 12, 2002)]
    [InlineData("", 0, 0, 0)]
    public void ConvertFilterNameToCustomDOBFilterText_CheckOutcome(string dobValue, int day, int month, int year)
    {
        // Arrange Act
        PupilHelper.ConvertFilterNameToCustomDOBFilterText(dobValue, out int dayOut, out int monthOut, out int yearOut);

        // Assert
        Assert.Equal(day, dayOut);
        Assert.Equal(month, monthOut);
        Assert.Equal(year, yearOut);
    }
}
