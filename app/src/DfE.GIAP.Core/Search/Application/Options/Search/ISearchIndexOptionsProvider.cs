namespace DfE.GIAP.Core.Search.Application.Options.Search;

public interface ISearchIndexOptionsProvider
{
    SearchIndexOptions GetOptions(string key);
}
