namespace DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilCtf;

public record DownloadPupilCtfRequest(
    IEnumerable<string> SelectedPupils,
    bool IsEstablishment,
    string LocalAuthoriyNumber,
    string EstablishmentNumber) : IUseCaseRequest<DownloadPupilCtfResponse>;
