using DfE.GIAP.Core.Downloads.Application.Enums;

namespace DfE.GIAP.Core.Downloads.Application.UseCases.GetAvailableDatasetsForPupils;

public record GetAvailableDatasetsForPupilsResponse(IEnumerable<AvailableDatasetResult> AvailableDatasets);

public record AvailableDatasetResult(Dataset Dataset, bool HasData, bool CanDownload);
