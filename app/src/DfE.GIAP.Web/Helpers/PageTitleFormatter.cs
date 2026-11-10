namespace DfE.GIAP.Web.Helpers;

public static class PageTitleFormatter
{
    private const string ServiceTitle = "Get information about pupils - GOV.UK";

    public static string Format(object title, bool isModelStateValid, bool isPost)
    {
        string pageTitle = title?.ToString();
        if (string.IsNullOrEmpty(pageTitle))
        {
            return ServiceTitle;
        }

        if (!isModelStateValid && !pageTitle.StartsWith("Error: "))
        {
            pageTitle = "Error: " + pageTitle;
        }
        else if (isPost && !pageTitle.StartsWith("Results: "))
        {
            pageTitle = "Results: " + pageTitle;
        }

        return $"{pageTitle} - {ServiceTitle}";
    }
}
