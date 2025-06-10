namespace DfE.GIAP.Core.Content.Application.Options.Extensions;
public static class PageContextOptionsExtensions
{
    public static IEnumerable<PageContentOption> TryGetPageContentOptionWithPageKey(this PageContentOptions contentOptions, string pageKey)
    {
        if (!contentOptions.Content.TryGetValue(pageKey, out IEnumerable<PageContentOption>? option))
        {
            throw new ArgumentException($"unable to find options for pageKey {pageKey}");
        }
        return option;
    }
}
