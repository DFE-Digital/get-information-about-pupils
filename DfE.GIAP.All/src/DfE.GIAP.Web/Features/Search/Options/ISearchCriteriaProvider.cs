using DfE.GIAP.Core.Search.Application.Models.Search;

namespace DfE.GIAP.Web.Features.Search.Options;
public interface ISearchCriteriaProvider
{
    SearchCriteria GetCriteria(string key);
}
