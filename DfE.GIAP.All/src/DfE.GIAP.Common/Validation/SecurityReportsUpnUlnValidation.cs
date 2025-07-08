using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Helpers;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DfE.GIAP.Common.Validation
{
    public class SecurityReportsUpnUlnValidation : ValidationAttribute
    {
        protected override ValidationResult IsValid(object x, ValidationContext context)
        {
            if (x == null)
            {
                return GetValidationResultError("EmptyUpnOrUln", SecurityReportsConstants.SecurityReportsByUpnEmptyUpnOrUln);
            }

            string upnUlnParam = SecurityHelper.SanitizeText(x.ToString());

            IList<string> invalidFormatList = new List<string>();

            var upnUlnList = ValidationHelper.FormatUPNULNSearchInput(upnUlnParam);

            foreach (var upnUln in upnUlnList)
            {
                if (!ValidationHelper.IsValidUpn(upnUln) && !ValidationHelper.IsValidUln(upnUln))
                {
                    invalidFormatList.Add(upnUln);
                }
            }

            if (invalidFormatList.Count > 0)
            {
                return GetValidationResultError("InvalidUpnUlnFormat", SecurityReportsConstants.SecurityReportsInvalidUpn + SecurityReportsConstants.SecurityReportsHTMLBRTag + SecurityReportsConstants.SecurityReportsInvalidUln);
            }

            return ValidationResult.Success;
        }

        private static ValidationResult GetValidationResultError(string key, string text)
        {
            return new ValidationResult($"{text}", new string[] { $"{key}" });
        }
    }
}
