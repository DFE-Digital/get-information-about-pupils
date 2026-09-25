using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DfE.GIAP.Web.Helpers.Html;

public static class HtmlHelper
{
    /// <summary>
    /// Generates a CSS class string for a form field based on its validation state.
    /// </summary>
    /// <remarks>This method checks the <see cref="ModelStateDictionary"/> for validation errors associated
    /// with the specified field. If errors are found, the <paramref name="errorClass"/> is appended to the <paramref
    /// name="baseClasses"/>.</remarks>
    /// <param name="htmlHelper">The <see cref="IHtmlHelper"/> instance used to access the view's model state.</param>
    /// <param name="field">The name of the form field to check for validation errors.</param>
    /// <param name="baseClasses">The base CSS classes to apply to the field.</param>
    /// <param name="errorClass">The CSS class to append if the field has validation errors.</param>
    /// <returns>A string containing the base CSS classes, with the error class appended if the specified field has validation
    /// errors.</returns>
    public static string ClassWithError(this IHtmlHelper htmlHelper, string field, string baseClasses, string errorClass)
    {
        ModelStateDictionary modelState = htmlHelper.ViewData.ModelState;
        return modelState[field]?.Errors?.Count > 0 ? $"{baseClasses} {errorClass}" : baseClasses;
    }
}
