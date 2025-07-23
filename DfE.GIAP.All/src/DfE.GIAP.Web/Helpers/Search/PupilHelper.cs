using System.Globalization;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Web.Constants;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DfE.GIAP.Web.Helpers.Search;

public static class PupilHelper
{

    public static bool CheckIfStarredPupil(string selectedPupil) =>
        selectedPupil?.Contains(Global.EncodedSuffixMarker) ?? false;


    public static bool CheckIfStarredPupil(string[] selectedPupils)
    {
        if (selectedPupils is null || selectedPupils.Length < 1)
            return false;

        foreach (var item in selectedPupils.ToList())
        {
            if (item.Contains(Global.EncodedSuffixMarker))
            {
                return true;
            }
        }

        return false;
    }

    public static string GenerateValidationMessageUpnSearch(ModelStateDictionary modelState)
    {
        string errorMessage = string.Empty;

        if (modelState.ErrorCount == 0)
        {
            return errorMessage;
        }

        foreach (ModelStateEntry state in modelState.Values)
        {
            foreach (ModelError error in state.Errors)
            {
                if (error.ErrorMessage.Contains("<span style='display:none'>1</span>")
                    || error.ErrorMessage.Contains("You have not entered any UPNs"))
                {
                    errorMessage = Messages.Search.Errors.EnterUPNs;
                }
                else if (error.ErrorMessage.Contains("<span style='display:none'>2</span>")
                    || error.ErrorMessage.Contains($"UPNs have been entered, please review and reduce to the maximum of"))
                {
                    errorMessage = Messages.Search.Errors.TooManyUPNs;
                }
                else if (error.ErrorMessage.Contains("<span style='display:none'>3</span>"))
                {
                    errorMessage = Messages.Search.Errors.UPNLength;
                }
                else if (error.ErrorMessage.Contains("<span style='display:none'>4</span>"))
                {
                    errorMessage = Messages.Search.Errors.UPNFormat;
                }
                else if (error.ErrorMessage.Contains("<span style='display:none'>5</span>")
                    || error.ErrorMessage.Contains("The following UPN(s) are duplicated"))
                {
                    errorMessage = Messages.Search.Errors.UPNMustBeUnique;
                }
            }
        }
        return errorMessage;
    }

    public static string GenerateValidationMessageUlnSearch(ModelStateDictionary modelState)
    {
        string errorMessage = string.Empty;

        if (modelState.ErrorCount == 0)
            return errorMessage;

        foreach (var state in modelState.Values)
        {
            foreach (var error in state.Errors)
            {
                if (error.ErrorMessage.Contains("<span style='display:none'>1</span>")
                    || error.ErrorMessage.Contains("You have not entered any ULNs"))
                {
                    errorMessage = Messages.Search.Errors.EnterULNs;
                }
                else if (error.ErrorMessage.Contains("<span style='display:none'>2</span>")
                    || error.ErrorMessage.Contains($"ULNs have been entered, please review and reduce to the maximum of"))
                {
                    errorMessage = Messages.Search.Errors.TooManyULNs;
                }
                else if (error.ErrorMessage.Contains("<span style='display:none'>3</span>"))
                {
                    errorMessage = Messages.Search.Errors.ULNLength;
                }
                else if (error.ErrorMessage.Contains("<span style='display:none'>4</span>"))
                {
                    errorMessage = Messages.Search.Errors.ULNFormat;
                }
            }
        }
        return errorMessage;
    }

    public static string SwitchGenderToParseName(this string genderValue) =>
        genderValue switch
        {
            "F" => "Female",
            "M" => "Male",
            _ => "Unknown"
        };

    public static string SwitchSexToParseName(this string sexValue) =>
        sexValue switch
        {
            "F" => "Female",
            "M" => "Male",
            _ => sexValue
        };

    public static void ConvertFilterNameToCustomDOBFilterText(string dobValue, out int day, out int month, out int year)
    {
        var dobParts = dobValue.Split("/");
        switch (dobParts.Length)
        {
            case 1:
                day = month = 0;
                int.TryParse(dobParts[0], out year);
                break;
            case 2:
                day = 0;
                int.TryParse(dobParts[0], out month);
                int.TryParse(dobParts[1], out year);
                break;
            case 3:
                int.TryParse(dobParts[0], out day);
                int.TryParse(dobParts[1], out month);
                int.TryParse(dobParts[2], out year);
                break;
            default:
                day = month = year = 0;
                break;
        }
    }

    public static bool IsValidateDate(string tempDate)
    {
        DateTime fromDateValue;
        var formats = new[] { "dd/MM/yyyy", "yyyy-MM-dd" };
        if (DateTime.TryParseExact(tempDate, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out fromDateValue))
        {
            return true;
        }

        return false;
    }
}
