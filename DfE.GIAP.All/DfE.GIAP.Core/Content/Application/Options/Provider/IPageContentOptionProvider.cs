namespace DfE.GIAP.Core.Content.Application.Options.Provider;
public interface IPageContentOptionProvider
{
    PageContentOption GetPageContentOptionWithPageKey(string pageKey);
}
