using Azure.Search.Documents;
namespace DfE.GIAP.Core.MyPupils.Application.Search.Provider;
internal interface ISearchClientProvider
{
    SearchClient GetClientByKey(string name); // TODO currently leaking AzureCogSearch impl
}
