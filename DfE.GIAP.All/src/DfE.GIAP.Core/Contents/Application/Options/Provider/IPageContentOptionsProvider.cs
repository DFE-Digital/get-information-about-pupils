using DfE.GIAP.Core.Contents.Application.Options;

namespace DfE.GIAP.Core.Contents.Application.Options.Provider;

/// <summary>
/// Defines a contract for retrieving page content options based on a page key.
/// </summary>
public interface IPageContentOptionsProvider
{
    /// <summary>
    /// Retrieves the <see cref="PageContentOption"/> associated with the specified page key.
    /// </summary>
    /// <param name="pageKey">The key identifying the page.</param>
    /// <returns>The <see cref="PageContentOption"/> for the given key.</returns>
    PageContentOption GetPageContentOptionWithPageKey(string pageKey);
}
