using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Downloads.Application.DatasetCheckers;
using DfE.GIAP.Core.Downloads.Application.Enums;

namespace DfE.GIAP.Core.Downloads.Application.UseCases.GetAvailableDatasetsForPupils;

public class GetAvailableDatasetsForPupilsUseCase : IUseCase<GetAvailableDatasetsForPupilsRequest, GetAvailableDatasetsForPupilsResponse>
{
    private readonly IDatasetAvailabilityCheckerFactory _factory;
    private readonly IDatasetAccessEvaluator _accessEvaluator;

    public GetAvailableDatasetsForPupilsUseCase(
        IDatasetAvailabilityCheckerFactory factory,
        IDatasetAccessEvaluator accessEvaluator)
    {
        _factory = factory;
        _accessEvaluator = accessEvaluator;
    }


    public async Task<GetAvailableDatasetsForPupilsResponse> HandleRequestAsync(GetAvailableDatasetsForPupilsRequest request)
    {
        IDatasetAvailabilityChecker datasetChecker = _factory.GetDatasetChecker(request.DownloadType);
        IEnumerable<Datasets> availableDatasets = await datasetChecker.GetAvailableDatasetsAsync(request.SelectedPupils);

        IReadOnlyCollection<Datasets> supportedDatasets = DownloadDatasetMap.GetSupportedDatasets(request.DownloadType);

        IEnumerable<AvailableDatasetResult> results = supportedDatasets.Select(dataset => new AvailableDatasetResult(
            Dataset: dataset,
            HasData: availableDatasets.Contains(dataset),
            CanDownload: availableDatasets.Contains(dataset) && _accessEvaluator.CanDownload(request.AuthorisationContext, dataset)
        ));

        return new GetAvailableDatasetsForPupilsResponse(results);
    }
}
