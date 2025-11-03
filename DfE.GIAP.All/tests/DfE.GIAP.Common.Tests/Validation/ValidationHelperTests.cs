using DfE.GIAP.Common.Helpers;
using System.Collections.Generic;
using Xunit;

namespace DfE.GIAP.Common.Tests.Validation
{
    public class ValidationHelperTests
    {
        [Fact]
        public void IsValidUpnLength_Check()
        {
            // Arrange
            string upn = "0123456789012";

            // Act
            var isSuccess = ValidationHelper.UPNLengthValidator(upn);
            // Assert
            Assert.True(isSuccess);
        }

        [Fact]
        public void IsNotValidUpnLength_Check()
        {
            // Arrange
            string upn = "0123456789012345";

            // Act
            var isSuccess = ValidationHelper.UPNLengthValidator(upn);
            // Assert
            Assert.False(isSuccess);
        }

        [Fact]
        public void IsValidUpnRegularExpression_Check()
        {
            // Arrange
            string upn = "A01234567890C";

            // Act
            var isSuccess = ValidationHelper.UPNRegexValidator(upn);
            // Assert
            Assert.True(isSuccess);
        }

        [Fact]
        public void IsNotValidUpnRegularExpression_Check()
        {
            // Arrange
            string upn = "0123456789xxx";

            // Act
            var isSuccess = ValidationHelper.UPNRegexValidator(upn);
            // Assert
            Assert.False(isSuccess);
        }


        [Fact]
        public void IsValidUpnFormat_Check()
        {
            // Arrange
            string upn = "A09876543210B";

            // Act
            var isSuccess = ValidationHelper.IsValidUpn(upn);
            // Assert
            Assert.True(isSuccess);
        }

        [Fact]
        public void IsNot_A_ValidUpnFormat_Check()
        {
            // Arrange
            string upn = "0123456789xxx";

            // Act
            var isSuccess = ValidationHelper.IsValidUpn(upn);
            // Assert
            Assert.False(isSuccess);
        }

        [Fact]
        public void IsValidUlnLength_Check()
        {
            // Arrange
            string uln = "0123456789";

            // Act
            var isSuccess = ValidationHelper.ULNLengthValidator(uln);
            // Assert
            Assert.True(isSuccess);
        }

        [Fact]
        public void IsNotValidUlnLength_Check()
        {
            // Arrange
            string uln = "0123456789101";

            // Act
            var isSuccess = ValidationHelper.ULNLengthValidator(uln);
            // Assert
            Assert.False(isSuccess);
        }

        [Fact]
        public void IsValidUlnRegularExpression_Check()
        {
            // Arrange
            string uln = "0123456789";

            // Act
            var isSuccess = ValidationHelper.ULNRegexValidator(uln);
            // Assert
            Assert.True(isSuccess);
        }

        [Fact]
        public void IsNotValidUlnRegularExpression_Check()
        {
            // Arrange
            string uln = "0123456789xxx";

            // Act
            var isSuccess = ValidationHelper.ULNRegexValidator(uln);
            // Assert
            Assert.False(isSuccess);
        }


        [Fact]
        public void IsValidUlnF_Check()
        {
            // Arrange
            string uln = "9999375358";

            // Act
            var isSuccess = ValidationHelper.IsValidUln(uln);
            // Assert
            Assert.True(isSuccess);
        }

        [Fact]
        public void IsNot_A_ValidUlnFormat_Check()
        {
            // Arrange
            string uln = "0123456789";

            // Act
            var isSuccess = ValidationHelper.IsValidUln(uln);
            // Assert
            Assert.False(isSuccess);
        }

        [Fact]
        public void GetDuplicates_Given_List_Of_Ulns_Or_Upns_Returns_Duplicates()
        {
            // Arrange
            List<string> listOfIdentifiers = new List<string>() { "9999730830", "9999730831", "9999730831" };

            // Act
            var duplicates = ValidationHelper.GetDuplicates(listOfIdentifiers);

            // Assert
            Assert.True(duplicates.Count == 1);
        }
    }
}
