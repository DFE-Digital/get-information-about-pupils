using DfE.GIAP.Common.Constants.Messages.Common;
using DfE.GIAP.Core.Models.Editor;
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
                return new ValidationResult(CommonErrorMessages.DocumentRequired);
            }
            return null;
        }
    }
}
