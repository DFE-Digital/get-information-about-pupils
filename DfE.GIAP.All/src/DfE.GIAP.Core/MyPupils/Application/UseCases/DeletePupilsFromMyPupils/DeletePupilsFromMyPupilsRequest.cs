using DfE.GIAP.Core.Common.Application;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
public record DeletePupilsFromMyPupilsRequest(
    string UserId, // AuthorisationContext contains UserId which is necessary with CosmosDB to retrieve the User to filter and delete the pupils selected
    IEnumerable<string> DeletePupilUpns,
    bool DeleteAll) : IUseCaseRequest;
