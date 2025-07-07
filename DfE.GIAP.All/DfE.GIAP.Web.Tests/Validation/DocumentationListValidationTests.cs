using DfE.GIAP.Common.Constants.Messages.Common;
using DfE.GIAP.Core.Models.Editor;
using DfE.GIAP.Web.Validation;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace DfE.GIAP.Web.Tests.Validation
{
    public class DocumentationListValidationTests
    {
        [Fact]
        public void IsValid_returns_null_if_valid()
        {
            // Arrange
            var document = new Document() { DocumentId = "1234" };
            var customValidationAttribute = new DocumentationListValidation();

            // Act
            var result = customValidationAttribute.GetValidationResult(document, new ValidationContext(document));

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void IsValid_returns_error_message_if_invalid()
        {
            // Arrange
            var document = new Document();
            var customValidationAttribute = new DocumentationListValidation();

            // Act
            var result = customValidationAttribute.GetValidationResult(document, new ValidationContext(document));

            // Assert
            Assert.IsType<ValidationResult>(result);
            Assert.Equal(CommonErrorMessages.DocumentRequired, result.ErrorMessage);
        }
    }
}
