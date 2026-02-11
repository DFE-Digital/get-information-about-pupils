using DfE.GIAP.Core.Downloads.Application.Availability.Handlers;
using DfE.GIAP.Core.Downloads.Application.Enums;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Repositories;
using DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.Downloads.Application.Availability.Handlers;

public sealed class NationalPupilDatasetHandlerTests
{
    [Fact]
    public async Task GetAvailableDatasetsAsync_ReturnsEmpty_When_NoPupilsHaveRelevantData()
    {
        List<NationalPupil> pupils = new()
        {
            NationalPupilTestDoubles.Create(),
        };

        INationalPupilReadOnlyRepository repository = NationalPupilRepositoryReadOnlyTestDouble.WithPupils(pupils);
        NationalPupilDatasetHandler handler = new(repository);

        IEnumerable<Dataset> result = await handler
            .GetAvailableDatasetsAsync(new[] { "abc", "def" });

        Assert.Empty(result);
    }

    [Theory]
    [InlineData(Dataset.KS1)]
    [InlineData(Dataset.KS2)]
    [InlineData(Dataset.KS4)]
    [InlineData(Dataset.MTC)]
    [InlineData(Dataset.Phonics)]
    [InlineData(Dataset.EYFSP)]
    [InlineData(Dataset.Census_Autumn)]
    [InlineData(Dataset.Census_Spring)]
    [InlineData(Dataset.Census_Summer)]
    public async Task GetAvailableDatasetsAsync_ReturnsExpectedDataset_WhenRelevantDataIsPresent(Dataset expectedDataset)
    {
        NationalPupil pupil = CreatePupilWithDataset(expectedDataset);
        NationalPupilDatasetHandler handler = CreateHandlerWithPupils(pupil);

        IEnumerable<Dataset> result = await handler.GetAvailableDatasetsAsync(new[] { "abc" });

        Assert.Contains(expectedDataset, result);
    }

    private static NationalPupilDatasetHandler CreateHandlerWithPupils(params NationalPupil[] pupils) =>
        new(NationalPupilRepositoryReadOnlyTestDouble.WithPupils(pupils.ToList()));

    private static NationalPupil CreatePupilWithDataset(Dataset dataset) => dataset switch
    {
        Dataset.KS1 => NationalPupilTestDoubles.Create(includeKeyStages: true),
        Dataset.KS2 => NationalPupilTestDoubles.Create(includeKeyStages: true),
        Dataset.KS4 => NationalPupilTestDoubles.Create(includeKeyStages: true),
        Dataset.MTC => NationalPupilTestDoubles.Create(includeMtc: true),
        Dataset.Phonics => NationalPupilTestDoubles.Create(includePhonics: true),
        Dataset.EYFSP => NationalPupilTestDoubles.Create(includeEyfsp: true),
        Dataset.Census_Autumn => NationalPupilTestDoubles.Create(includeCensus: true),
        Dataset.Census_Spring => NationalPupilTestDoubles.Create(includeCensus: true),
        Dataset.Census_Summer => NationalPupilTestDoubles.Create(includeCensus: true),
        _ => throw new ArgumentOutOfRangeException(nameof(dataset), $"Unsupported dataset: {dataset}")
    };
}
