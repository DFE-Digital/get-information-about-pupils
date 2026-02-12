using System.ComponentModel.DataAnnotations;
using DfE.GIAP.Web.Helpers;
using DfE.GIAP.Web.ViewModels.Search;
using Xunit;

namespace DfE.GIAP.Common.Tests.Validation;

public class SearchLearnerNumberValidationAttributeTests
{
    [Fact]
    public void IsValidLearnerNumber_Check()
    {
        // Arrange
        var ulnString = "H356210811018";
        var vm = new LearnerNumberSearchViewModel() { LearnerNumber = ulnString };
        var customValidationAttribute = new SearchLearnerNumberValidation();

        // Act
        var isSuccess = customValidationAttribute.GetValidationResult(ulnString, new ValidationContext(vm));

        // Assert
        Assert.True(isSuccess == ValidationResult.Success);
    }

    [Fact]
    public void NoLearnerNumbersError()
    {
        // Arrange
        var vm = new LearnerNumberSearchViewModel() { LearnerNumberLabel = "UPN" };
        var customValidationAttribute = new SearchLearnerNumberValidation();

        // Act
        var validationResult = customValidationAttribute.GetValidationResult(null, new ValidationContext(vm));

        // Assert
        Assert.False(validationResult == ValidationResult.Success);
        Assert.Equal($"You have not entered any {vm.LearnerNumberLabel}s", validationResult.ErrorMessage);
    }
}
