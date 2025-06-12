namespace DfE.GIAP.Core.Content.Application.Options.Provider;
public interface IPageContentOptionsProvider
{
    PageContentOption GetPageContentOptionWithPageKey(string pageKey);
}
