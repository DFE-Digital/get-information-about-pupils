using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DfE.GIAP.Web.Helpers.Html;

public static class HtmlHelper
{
    public static string ClassWithError(this IHtmlHelper htmlHelper, string field, string baseClass, string errorClass)
    {
        ModelStateDictionary modelState = htmlHelper.ViewData.ModelState;
        return modelState[field]?.Errors?.Count > 0 ? $"{baseClass} {errorClass}" : baseClass;
    }
}
