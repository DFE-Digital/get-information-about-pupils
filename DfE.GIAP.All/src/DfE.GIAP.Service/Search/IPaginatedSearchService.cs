using DfE.GIAP.Common.Enums;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Domain.Search.Learner;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DfE.GIAP.Service.Search
{
    public interface IPaginatedSearchService
    {
        Task<PaginatedResponse> GetPage(
            string searchText,
            Dictionary<string, string[]> filters,
            int pageSize,
            int pageNumber,
            AzureSearchIndexType indexType,
            AzureSearchQueryType queryType,
            AzureFunctionHeaderDetails azureFunctionHeaderDetails,
            string sortField = "",
            string sortDirection = "");
    }
}
