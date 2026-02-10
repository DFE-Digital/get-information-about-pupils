using DfE.GIAP.Core.Search.Application.Models.Sort;
using DfE.GIAP.Core.Search.Application.Options.Sort;

namespace DfE.GIAP.Web.Features.Search.Shared.Sort;

public interface ISortOrderFactory
{
    SortOrder Create(SortOptions options, (string? field, string? direction) sort);
}