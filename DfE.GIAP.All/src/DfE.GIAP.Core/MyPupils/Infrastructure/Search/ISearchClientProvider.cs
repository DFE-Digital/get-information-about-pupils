using Azure.Search.Documents;
namespace DfE.GIAP.Core.MyPupils.Infrastructure.Search;
public interface ISearchClientProvider
{
    Task<List<TResult>> InvokeSearchAsync<TResult>(
        string searchIndexKey,
        SearchOptions options); // TODO currently leaking AzureCogSearch impl
}
