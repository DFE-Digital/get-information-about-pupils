using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Downloads.Application.Enums;

namespace DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilDatasets;

public record DownloadPupilDataRequest(
    IEnumerable<string> SelectedPupils,
    IEnumerable<Dataset> SelectedDatasets,
    DownloadType DownloadType,
    FileFormat FileFormat) : IUseCaseRequest<DownloadPupilDataResponse>;
