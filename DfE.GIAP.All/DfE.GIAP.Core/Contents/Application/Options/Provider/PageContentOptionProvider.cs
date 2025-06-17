using DfE.GIAP.Core.Contents.Application.Options;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Core.Contents.Application.Options.Provider;


/// <summary>
/// Provides access to configured <see cref="PageContentOption"/> instances based on page keys.
/// </summary>
public class PageContentOptionProvider : IPageContentOptionsProvider
{
    private readonly PageContentOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="PageContentOptionProvider"/> class.
    /// </summary>
    /// <param name="options">The configured page content options.</param>
    /// <exception cref="ArgumentNullException">Thrown if the options or its value is null.</exception>
    public PageContentOptionProvider(IOptions<PageContentOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(options.Value);
        _options = options.Value;
    }

    /// <summary>
    /// Retrieves the <see cref="PageContentOption"/> associated with the specified page key.
    /// </summary>
    /// <param name="pageKey">The key identifying the page.</param>
    /// <returns>The corresponding <see cref="PageContentOption"/>.</returns>
    /// <exception cref="ArgumentException">Thrown if the page key is not found in the options.</exception>
    public PageContentOption GetPageContentOptionWithPageKey(string pageKey)
    {
        if (!_options.TryGetValue(pageKey, out PageContentOption? option))
        {
            throw new ArgumentException($"Could not find PageContentOptions from pageKey: {pageKey}");
        }
        return option;
    }
}
