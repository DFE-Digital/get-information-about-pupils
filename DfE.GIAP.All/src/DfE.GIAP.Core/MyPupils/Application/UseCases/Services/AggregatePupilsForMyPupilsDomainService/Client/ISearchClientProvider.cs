using Azure.Search.Documents;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.Services.AggregatePupilsForMyPupilsDomainService.Client;

internal interface ISearchClientProvider
{
    SearchClient GetClientByKey(string name);
}
