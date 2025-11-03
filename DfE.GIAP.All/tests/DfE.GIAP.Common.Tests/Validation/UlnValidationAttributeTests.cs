using DfE.GIAP.Common.Validation;
using Xunit;

namespace DfE.GIAP.Common.Tests.Validation
{
    public class UlnValidationAttributeTests
    {
        [Fact]
        public void IsValidUln_Check()
        {
            // Arrange
            object uln = "9999375358";
            var customValidationAttribute = new SecurityReportsUpnUlnValidation();

            // Act
            var isSuccess = customValidationAttribute.IsValid(uln);
            // Assert
            Assert.True(isSuccess);
        }

        [Fact]
        public void IsNot_A_ValidUln_Check()
        {
            // Arrange
            object uln = null;
            var customValidationAttribute = new SecurityReportsUpnUlnValidation();

            // Act
            var isSuccess = customValidationAttribute.IsValid(uln);
            // Assert
            Assert.False(isSuccess);
        }

        [Fact]
        public void Less_Than_10_Char_Uln_Check()
        {
            // Arrange
            object uln = "01234";
            var customValidationAttribute = new SecurityReportsUpnUlnValidation();

            // Act
            var isSuccess = customValidationAttribute.IsValid(uln);
            // Assert
            Assert.False(isSuccess);
        }

        [Fact]
        public void Wrong_Format_Uln_Check()
        {
            // Arrange
            object uln = "qwertyuiop";
            var customValidationAttribute = new SecurityReportsUpnUlnValidation();

            // Act
            var isSuccess = customValidationAttribute.IsValid(uln);
            // Assert
            Assert.False(isSuccess);
        }
    }
}
