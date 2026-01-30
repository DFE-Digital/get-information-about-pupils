using DfE.GIAP.Core.Search.Application.Models.Search;

namespace DfE.GIAP.Core.Search.Application.Services;
public interface ISearchCriteriaProvider
{
    SearchCriteria GetCriteria(string key);
}
