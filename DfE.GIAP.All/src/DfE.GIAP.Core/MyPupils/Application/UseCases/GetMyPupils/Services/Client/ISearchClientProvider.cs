using Azure.Search.Documents;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Services.Client;

internal interface ISearchClientProvider
{
    SearchClient GetClientByKey(string name);
}
