using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Downloads.Application.Availability.Access.Policies;
using DfE.GIAP.Core.Downloads.Application.Enums;

namespace DfE.GIAP.Core.Downloads.Application.UseCases.GetAvailableDatasetsForPupils;

public record GetAvailableDatasetsForPupilsRequest(
    PupilDownloadType DownloadType,
    IEnumerable<string> SelectedPupils,
    IAuthorisationContext AuthorisationContext) : IUseCaseRequest<GetAvailableDatasetsForPupilsResponse>;
