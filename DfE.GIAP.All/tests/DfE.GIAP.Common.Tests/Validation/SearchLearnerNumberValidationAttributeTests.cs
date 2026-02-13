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
        string ulnString = "H356210811018";
        LearnerNumberSearchViewModel vm = new LearnerNumberSearchViewModel() { LearnerNumber = ulnString };
        SearchLearnerNumberValidation customValidationAttribute = new SearchLearnerNumberValidation();

        // Act
        ValidationResult? isSuccess = customValidationAttribute.GetValidationResult(ulnString, new ValidationContext(vm));

        // Assert
        Assert.True(isSuccess == ValidationResult.Success);
    }

    [Fact]
    public void NoLearnerNumbersError()
    {
        // Arrange
        LearnerNumberSearchViewModel vm = new LearnerNumberSearchViewModel() { LearnerNumberLabel = "UPN" };
        SearchLearnerNumberValidation customValidationAttribute = new SearchLearnerNumberValidation();

        // Act
        ValidationResult? validationResult = customValidationAttribute.GetValidationResult(null, new ValidationContext(vm));

        // Assert
        Assert.False(validationResult == ValidationResult.Success);
        Assert.Equal($"You have not entered any {vm.LearnerNumberLabel}s", validationResult.ErrorMessage);
    }
}
