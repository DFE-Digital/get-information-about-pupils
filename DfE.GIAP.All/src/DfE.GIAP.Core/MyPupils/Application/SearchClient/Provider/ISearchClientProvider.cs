using Azure.Search.Documents;

namespace DfE.GIAP.Core.MyPupils.Application.SearchClient.Provider;

internal interface ISearchClientProvider
{
    SearchClient GetClientByKey(string name);
}
