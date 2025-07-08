using DfE.GIAP.Common.Validation;
using Xunit;

namespace DfE.GIAP.Common.Tests.Validation
{
    [Trait("Validator", "Upn Attribute")]
    public class UpnValidationAttributeTests
    {
        [Fact]
        public void IsValidUpn_Check()
        {
            // Arrange
            object upn = "A111111111111";
            var customValidationAttribute = new SecurityReportsUpnUlnValidation();
            
            // Act
            var isSuccess = customValidationAttribute.IsValid(upn);
            // Assert
            Assert.True(isSuccess);
        }

        [Fact]
        public void IsNot_A_ValidUpn_Check()
        {
            // Arrange
            object upn = null;
            var customValidationAttribute = new SecurityReportsUpnUlnValidation();

            // Act
            var isSuccess = customValidationAttribute.IsValid(upn);
            // Assert
            Assert.False(isSuccess);
        }

        [Fact]
        public void Less_Than_13_Char_Upn_Check()
        {
            // Arrange
            object upn = "A1111111111";
            var customValidationAttribute = new SecurityReportsUpnUlnValidation();

            // Act
            var isSuccess = customValidationAttribute.IsValid(upn);
            // Assert
            Assert.False(isSuccess);
        }

        [Fact]
        public void Wrong_Format_Upn_Check()
        {
            // Arrange
            object upn = "a1x1111111a";
            var customValidationAttribute = new SecurityReportsUpnUlnValidation();

            // Act
            var isSuccess = customValidationAttribute.IsValid(upn);
            // Assert
            Assert.False(isSuccess);
        }
    }
}
