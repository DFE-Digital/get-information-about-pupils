namespace DfE.GIAP.Core.Content.Application.Options.Extensions;
public static class PageContextOptionsExtensions
{
    public static PageContentOption GetPageContentOptionWithPageKey(this PageContentOptions contentOptions, string pageKey)
    {
        if (!contentOptions.TryGetValue(pageKey, out PageContentOption? option))
        {
            throw new ArgumentException($"unable to find options for pageKey {pageKey}");
        }
        return option;
    }
}
