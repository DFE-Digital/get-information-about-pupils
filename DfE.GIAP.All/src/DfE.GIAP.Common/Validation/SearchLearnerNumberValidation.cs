using DfE.GIAP.Common.Helpers;
using System.ComponentModel.DataAnnotations;

namespace DfE.GIAP.Common.Validation
{
    public class SearchLearnerNumberValidation : ValidationAttribute
    {
        private readonly string _MaximumLearnerNumbersPerSearch;

        public SearchLearnerNumberValidation(string MaximumLearnerNumbers)
        {
            _MaximumLearnerNumbersPerSearch = MaximumLearnerNumbers;
        }

        protected override ValidationResult IsValid(object x, ValidationContext context)
        {
            var property = context.ObjectType.GetProperty(_MaximumLearnerNumbersPerSearch);
            int.TryParse(property.GetValue(context.ObjectInstance, null).ToString(), out int propertyMaximumPerSearch);

            var learnerNumber = context.ObjectType.GetProperty("LearnerNumberLabel").GetValue(context.ObjectInstance, null);

            if (x == null)
            {
                return GetValidationResultError("EmptyUpn", $"You have not entered any {learnerNumber}s");
            }

            string upnParam = SecurityHelper.SanitizeText(x.ToString());

            var upnsList = ValidationHelper.FormatUPNULNSearchInput(upnParam);

            if (upnsList.Count > propertyMaximumPerSearch)
            {
                return GetValidationResultError("UpnExceededMaximumThreshold", $"More than {propertyMaximumPerSearch} {learnerNumber}s have been entered, please review and reduce to the maximum of {propertyMaximumPerSearch}");
            }

            return ValidationResult.Success;
        }

        private static ValidationResult GetValidationResultError(string key, string text)
        {
            return new ValidationResult($"{text}", new string[] { $"{key}" });
        }
    }
}