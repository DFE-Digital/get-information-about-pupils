using DfE.GIAP.Core.Models.Editor;
using DfE.GIAP.Web.Constants;
using System.ComponentModel.DataAnnotations;

namespace DfE.GIAP.Web.Validation
{
    public class DocumentationListValidation : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null) return null;
            var document = (Document)value;

            if (document.DocumentId == null)
            {
                return new ValidationResult(Messages.Common.Errors.AdminDocumentRequired);
            }
            return null;
        }
    }
}
