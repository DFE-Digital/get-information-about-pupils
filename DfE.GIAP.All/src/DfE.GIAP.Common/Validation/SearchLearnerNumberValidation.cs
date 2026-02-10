using DfE.GIAP.Common.Helpers;
using System.ComponentModel.DataAnnotations;

namespace DfE.GIAP.Common.Validation;

public class SearchLearnerNumberValidation : ValidationAttribute
{
    
    protected override ValidationResult IsValid(object x, ValidationContext context)
    {
        
        var learnerNumber = context.ObjectType.GetProperty("LearnerNumberLabel").GetValue(context.ObjectInstance, null);

        if (x == null)
        {
            return GetValidationResultError("EmptyUpn", $"You have not entered any {learnerNumber}s");
        }

        string upnParam = SecurityHelper.SanitizeText(x.ToString());

        ValidationHelper.FormatUPNULNSearchInput(upnParam);

        return ValidationResult.Success;
    }

    private static ValidationResult GetValidationResultError(string key, string text)
    {
        return new ValidationResult($"{text}", new string[] { $"{key}" });
    }
}
