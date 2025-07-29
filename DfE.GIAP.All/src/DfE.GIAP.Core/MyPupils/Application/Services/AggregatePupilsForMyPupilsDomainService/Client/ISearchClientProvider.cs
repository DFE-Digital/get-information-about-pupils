using Azure.Search.Documents;

namespace DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupilsDomainService.Client;

internal interface ISearchClientProvider
{
    SearchClient GetClientByKey(string name);
}
