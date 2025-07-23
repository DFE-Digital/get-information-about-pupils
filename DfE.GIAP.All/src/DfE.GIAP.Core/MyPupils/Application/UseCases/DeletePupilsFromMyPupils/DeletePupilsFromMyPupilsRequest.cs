using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Request;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
public record DeletePupilsFromMyPupilsRequest(
    IAuthorisationContext AuthorisationContext, // AuthorisationContext contains UserId which is necessary with CosmosDB to retrieve the User to filter and delete the pupils selected
    IEnumerable<string> PupilIdentifiers,
    bool DeleteAll) : IUseCaseRequest;
