using DfE.GIAP.Common.Validation;
using DfE.GIAP.Web.ViewModels.Search;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Xunit;

namespace DfE.GIAP.Common.Tests.Validation
{
    public class SearchLearnerNumberValidationAttributeTests
    {
        [Fact]
        public void IsValidLearnerNumber_Check()
        {
            // Arrange
            LearnerNumberSearchViewModel.MaximumLearnerNumbersPerSearch = 2;

            var ulnString = "H356210811018";
            var vm = new LearnerNumberSearchViewModel() { LearnerNumber = ulnString };
            var customValidationAttribute = new SearchLearnerNumberValidation("MaximumLearnerNumbersPerSearch");

            // Act
            var isSuccess = customValidationAttribute.GetValidationResult(ulnString, new ValidationContext(vm));

            // Assert
            Assert.True(isSuccess == ValidationResult.Success);
        }

        [Fact]
        public void NoLearnerNumbersError()
        {
            // Arrange
            LearnerNumberSearchViewModel.MaximumLearnerNumbersPerSearch = 2;

            var vm = new LearnerNumberSearchViewModel() { LearnerNumberLabel = "UPN" };
            var customValidationAttribute = new SearchLearnerNumberValidation("MaximumLearnerNumbersPerSearch");

            // Act
            var validationResult = customValidationAttribute.GetValidationResult(null, new ValidationContext(vm));

            // Assert
            Assert.False(validationResult == ValidationResult.Success);
            Assert.Equal($"You have not entered any {vm.LearnerNumberLabel}s", validationResult.ErrorMessage);
        }

        [Fact]
        public void MaximumThresholdReachedError()
        {
            // Arrange
            LearnerNumberSearchViewModel.MaximumLearnerNumbersPerSearch = 2;

            var ulnString = "9999375358\r\n9999375358\r\n9999375358";
            var vm = new LearnerNumberSearchViewModel() { LearnerNumber = ulnString, LearnerNumberLabel = "ULN" };
            var customValidationAttribute = new SearchLearnerNumberValidation("MaximumLearnerNumbersPerSearch");

            // Act
            var validationResult = customValidationAttribute.GetValidationResult(ulnString, new ValidationContext(vm));

            // Assert
            Assert.False(validationResult == ValidationResult.Success);
            Assert.Equal($"More than {LearnerNumberSearchViewModel.MaximumLearnerNumbersPerSearch} {vm.LearnerNumberLabel}s have been entered, please review and reduce to the maximum of {LearnerNumberSearchViewModel.MaximumLearnerNumbersPerSearch}", validationResult.ErrorMessage);
        }
    }
}