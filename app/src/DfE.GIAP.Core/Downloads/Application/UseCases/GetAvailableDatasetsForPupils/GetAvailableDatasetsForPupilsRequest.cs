using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Downloads.Application.Enums;
using DfE.GIAP.Core.Downloads.Application.UseCases.GetAvailableDatasetsForPupils.Availability.Access.Policies;

namespace DfE.GIAP.Core.Downloads.Application.UseCases.GetAvailableDatasetsForPupils;

public record GetAvailableDatasetsForPupilsRequest(
    PupilDownloadType DownloadType,
    IEnumerable<string> SelectedPupils,
    IAuthorisationContext AuthorisationContext) : IUseCaseRequest<GetAvailableDatasetsForPupilsResponse>;
