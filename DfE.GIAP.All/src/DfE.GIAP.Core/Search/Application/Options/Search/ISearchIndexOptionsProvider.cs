using DfE.GIAP.Core.Search.Application.Options.Search;

namespace DfE.GIAP.Web.Features.Search.Options.Search;

public interface ISearchIndexOptionsProvider
{
    SearchIndexOptions GetOptions(string key);
}
