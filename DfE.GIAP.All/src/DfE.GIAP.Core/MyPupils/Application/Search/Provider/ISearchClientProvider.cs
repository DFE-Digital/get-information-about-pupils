using Azure.Search.Documents;
namespace DfE.GIAP.Core.MyPupils.Application.Search.Provider;
public interface ISearchClientProvider
{
    Task<List<TResult>> InvokeSearchAsync<TResult>(
        string clientKey,
        SearchOptions options); // TODO currently leaking AzureCogSearch impl
}
