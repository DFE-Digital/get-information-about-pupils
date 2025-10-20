using DfE.GIAP.Core.Downloads.Application.Enums;

namespace DfE.GIAP.Core.Downloads.Application.UseCases.GetAvailableDatasetsForPupils;

public record GetAvailableDatasetsForPupilsResponse(IEnumerable<Datasets> AvailableDatasets);
