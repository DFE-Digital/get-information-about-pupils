using DfE.GIAP.Core.Content.Application.UseCases.GetContentByPageKeyUseCase;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Core.Content.Application.Options.Provider;
public class PageContentOptionProvider : IPageContentOptionsProvider
{
    private readonly PageContentOptions _options;

    public PageContentOptionProvider(IOptions<PageContentOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options.Value
            ?? throw new ArgumentNullException(nameof(options.Value));
    }

    public PageContentOption GetPageContentOptionWithPageKey(string pageKey)
    {
        if (!_options.TryGetValue(pageKey, out PageContentOption? option))
        {
            throw new ArgumentException($"Could not find PageContentOptions from pageKey: {pageKey}");
        }
        return option;
    }
}
