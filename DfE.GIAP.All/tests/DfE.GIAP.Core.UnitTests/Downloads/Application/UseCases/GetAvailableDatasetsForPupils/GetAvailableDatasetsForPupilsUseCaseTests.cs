using DfE.GIAP.Core.Downloads.Application.Datasets.Access.Policies;
using DfE.GIAP.Core.Downloads.Application.Datasets.Availability;
using DfE.GIAP.Core.Downloads.Application.Datasets.Availability.Handlers;
using DfE.GIAP.Core.Downloads.Application.Enums;
using DfE.GIAP.Core.Downloads.Application.UseCases.GetAvailableDatasetsForPupils;
using DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.Downloads.Application.UseCases.GetAvailableDatasetsForPupils;

public sealed class GetAvailableDatasetsForPupilsUseCaseTests
{
    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenFactoryIsNull()
    {
        IDatasetAccessEvaluator evaluator = DatasetAccessEvaluatorTestDouble.AllowAll();

        Assert.Throws<ArgumentNullException>(() =>
            new GetAvailableDatasetsForPupilsUseCase(null!, evaluator));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenEvaluatorIsNull()
    {
        IDatasetAvailabilityHandlerFactory factory = DatasetAvailabilityHandlerFactoryTestDouble.WithHandlers();

        Assert.Throws<ArgumentNullException>(() =>
            new GetAvailableDatasetsForPupilsUseCase(factory, null!));
    }

    [Fact]
    public async Task HandleRequestAsync_Returns_DatasetsWithAccessAndData()
    {
        IAuthorisationContext context = AuthorisationContextTestDouble.Create();

        IDatasetAvailabilityHandler handler = DatasetAvailabilityHandlerTestDouble.Create(
            DownloadType.FurtherEducation,
            new[] { Dataset.PP, Dataset.SEN });

        IDatasetAvailabilityHandlerFactory factory = DatasetAvailabilityHandlerFactoryTestDouble.WithHandler(
            DownloadType.FurtherEducation,
            handler);
        IDatasetAccessEvaluator evaluator = DatasetAccessEvaluatorTestDouble.AllowAll();

        GetAvailableDatasetsForPupilsUseCase sut = new(factory, evaluator);

        GetAvailableDatasetsForPupilsRequest request = new(DownloadType.FurtherEducation, new[] { "abc" }, context);
        GetAvailableDatasetsForPupilsResponse response = await sut.HandleRequestAsync(request);

        Assert.All(response.AvailableDatasets, r =>
        {
            Assert.True(r.HasData);
            Assert.True(r.CanDownload);
        });
    }

    [Fact]
    public async Task HandleRequestAsync_Returns_DatasetsWithDataButNoAccess()
    {
        IAuthorisationContext context = AuthorisationContextTestDouble.Create();
        IDatasetAvailabilityHandler handler = DatasetAvailabilityHandlerTestDouble.Create(
            DownloadType.FurtherEducation,
            new[] { Dataset.PP });

        IDatasetAvailabilityHandlerFactory factory = DatasetAvailabilityHandlerFactoryTestDouble.WithHandler(
            DownloadType.FurtherEducation,
            handler);

        IDatasetAccessEvaluator evaluator = DatasetAccessEvaluatorTestDouble.DenyAll();

        GetAvailableDatasetsForPupilsRequest request = new(DownloadType.FurtherEducation, new[] { "abc" }, context);
        GetAvailableDatasetsForPupilsUseCase sut = new(factory, evaluator);

        GetAvailableDatasetsForPupilsResponse response = await sut.HandleRequestAsync(request);

        AvailableDatasetResult result = response.AvailableDatasets.Single(r => r.Dataset == Dataset.PP);
        Assert.True(result.HasData);
        Assert.False(result.CanDownload);
    }

    [Fact]
    public async Task HandleRequestAsync_Returns_AllSupportedDatasets_WhenNoDataExists()
    {
        IAuthorisationContext context = AuthorisationContextTestDouble.Create();
        IDatasetAvailabilityHandler handler = DatasetAvailabilityHandlerTestDouble.Create(
            DownloadType.FurtherEducation,
            Enumerable.Empty<Dataset>());

        IDatasetAvailabilityHandlerFactory factory = DatasetAvailabilityHandlerFactoryTestDouble.WithHandler(
            DownloadType.FurtherEducation,
            handler);

        IDatasetAccessEvaluator evaluator = DatasetAccessEvaluatorTestDouble.AllowAll();

        GetAvailableDatasetsForPupilsRequest request = new(DownloadType.FurtherEducation, new[] { "abc" }, context);
        GetAvailableDatasetsForPupilsUseCase sut = new(factory, evaluator);

        GetAvailableDatasetsForPupilsResponse response = await sut.HandleRequestAsync(request);

        foreach (AvailableDatasetResult result in response.AvailableDatasets)
        {
            Assert.False(result.HasData);
            Assert.False(result.CanDownload);
        }
    }

    [Fact]
    public async Task HandleRequestAsync_CanDownloadIsTrue_OnlyForDatasetsWithDataAndAccess()
    {
        IAuthorisationContext context = AuthorisationContextTestDouble.Create();
        IDatasetAvailabilityHandler handler = DatasetAvailabilityHandlerTestDouble.Create(
            DownloadType.FurtherEducation,
            new[] { Dataset.PP, Dataset.SEN });

        IDatasetAvailabilityHandlerFactory factory = DatasetAvailabilityHandlerFactoryTestDouble.WithHandler(
            DownloadType.FurtherEducation,
            handler);

        IDatasetAccessEvaluator evaluator = DatasetAccessEvaluatorTestDouble.WithRule(
            (ctx, dataset) => dataset == Dataset.PP);

        GetAvailableDatasetsForPupilsRequest request = new(DownloadType.FurtherEducation, new[] { "abc" }, context);
        GetAvailableDatasetsForPupilsUseCase sut = new(factory, evaluator);

        GetAvailableDatasetsForPupilsResponse response = await sut.HandleRequestAsync(request);

        AvailableDatasetResult pp = response.AvailableDatasets.Single(r => r.Dataset == Dataset.PP);
        AvailableDatasetResult sen = response.AvailableDatasets.Single(r => r.Dataset == Dataset.SEN);

        Assert.True(pp.HasData);
        Assert.True(pp.CanDownload);

        Assert.True(sen.HasData);
        Assert.False(sen.CanDownload);
    }
}
