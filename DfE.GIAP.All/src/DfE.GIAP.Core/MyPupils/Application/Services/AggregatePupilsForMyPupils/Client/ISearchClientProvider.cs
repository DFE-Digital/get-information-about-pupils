using Azure.Search.Documents;

namespace DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils.Client;

internal interface ISearchClientProvider
{
    SearchClient GetClientByKey(string name);
}
