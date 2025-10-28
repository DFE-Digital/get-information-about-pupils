using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Downloads.Application.Datasets.Access.Policies;
using DfE.GIAP.Core.Downloads.Application.Datasets.Availability;
using DfE.GIAP.Core.Downloads.Application.Datasets.Availability.Handlers;
using DfE.GIAP.Core.Downloads.Application.Enums;

namespace DfE.GIAP.Core.Downloads.Application.UseCases.GetAvailableDatasetsForPupils;

public class GetAvailableDatasetsForPupilsUseCase : IUseCase<GetAvailableDatasetsForPupilsRequest, GetAvailableDatasetsForPupilsResponse>
{
    private readonly IDatasetAvailabilityHandlerFactory _datasetAvailabilityHandlerFactory;
    private readonly IDatasetAccessEvaluator _datasetAccessEvaluator;

    public GetAvailableDatasetsForPupilsUseCase(
        IDatasetAvailabilityHandlerFactory datasetAvailabilityFactory,
        IDatasetAccessEvaluator datasetAccessEvaluator)
    {
        _datasetAvailabilityHandlerFactory = datasetAvailabilityFactory;
        _datasetAccessEvaluator = datasetAccessEvaluator;
    }

    public async Task<GetAvailableDatasetsForPupilsResponse> HandleRequestAsync(GetAvailableDatasetsForPupilsRequest request)
    {
        IDatasetAvailabilityHandler datasetAvailabilityHandler = _datasetAvailabilityHandlerFactory.GetDatasetAvailabilityHandler(request.DownloadType);
        IEnumerable<Dataset> datasetsWithData = await datasetAvailabilityHandler.GetAvailableDatasetsAsync(request.SelectedPupils);

        IReadOnlyCollection<Dataset> supportedDatasets = AvailableDatasetsByDownloadType.GetSupportedDatasets(request.DownloadType);
        IEnumerable<AvailableDatasetResult> results = BuildDatasetResults(datasetsWithData, supportedDatasets, request.AuthorisationContext);

        return new GetAvailableDatasetsForPupilsResponse(results);
    }

    private IEnumerable<AvailableDatasetResult> BuildDatasetResults(
    IEnumerable<Dataset> datasetsWithData,
    IReadOnlyCollection<Dataset> supportedDatasets,
    IAuthorisationContext context)
    {
        return supportedDatasets.Select(dataset => new AvailableDatasetResult(
            Dataset: dataset,
            HasData: datasetsWithData.Contains(dataset),
            CanDownload: datasetsWithData.Contains(dataset) && _datasetAccessEvaluator.HasAccess(context, dataset)
        ));
    }
}
