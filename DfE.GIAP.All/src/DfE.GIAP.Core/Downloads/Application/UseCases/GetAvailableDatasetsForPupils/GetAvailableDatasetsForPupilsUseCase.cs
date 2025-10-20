using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Downloads.Application.DatasetCheckers;
using DfE.GIAP.Core.Downloads.Application.Enums;

namespace DfE.GIAP.Core.Downloads.Application.UseCases.GetAvailableDatasetsForPupils;

public class GetAvailableDatasetsForPupilsUseCase : IUseCase<GetAvailableDatasetsForPupilsRequest, GetAvailableDatasetsForPupilsResponse>
{
    private readonly IDatasetAvailabilityCheckerFactory _factory;

    public GetAvailableDatasetsForPupilsUseCase(IDatasetAvailabilityCheckerFactory datasetAvailabilityCheckerFactory)
    {
        ArgumentNullException.ThrowIfNull(datasetAvailabilityCheckerFactory);
        _factory = datasetAvailabilityCheckerFactory;
    }


    public async Task<GetAvailableDatasetsForPupilsResponse> HandleRequestAsync(GetAvailableDatasetsForPupilsRequest request)
    {
        IDatasetAvailabilityChecker datasetChecker = _factory.GetDatasetChecker(request.DownloadType);
        IEnumerable<Datasets> datasets = await datasetChecker.GetAvailableDatasetsAsync(request.SelectedPupils);
        return new GetAvailableDatasetsForPupilsResponse(datasets);
    }
}
