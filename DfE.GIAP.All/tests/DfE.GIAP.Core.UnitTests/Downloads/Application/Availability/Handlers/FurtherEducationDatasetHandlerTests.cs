using DfE.GIAP.Core.Downloads.Application.Datasets.Availability.Handlers;
using DfE.GIAP.Core.Downloads.Application.Enums;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Repositories;
using DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.Downloads.Application.Availability.Handlers;

public sealed class FurtherEducationDatasetHandlerTests
{
    [Fact]
    public async Task GetAvailableDatasetsAsync_ReturnsPP_When_PupilHasPupilPremiumData()
    {
        List<FurtherEducationPupil> pupils = new()
        {
            FurtherEducationPupilTestDoubles.Create(includePupilPremium: true)
        };

        IFurtherEducationReadOnlyRepository repository = FurtherEducationRepositoryReadOnlyTestDouble.WithPupils(pupils);
        FurtherEducationDatasetHandler sut = new(repository);

        IEnumerable<Dataset> result = await sut
            .GetAvailableDatasetsAsync(new[] { "abc" });

        Assert.Contains(Dataset.PP, result);
        Assert.DoesNotContain(Dataset.SEN, result);
    }

    [Fact]
    public async Task GetAvailableDatasetsAsync_ReturnsSEN_When_PupilHasSpecialEducationalNeedsData()
    {
        List<FurtherEducationPupil> pupils = new()
        {
            FurtherEducationPupilTestDoubles.Create(includeSen: true),
        };

        IFurtherEducationReadOnlyRepository repository = FurtherEducationRepositoryReadOnlyTestDouble.WithPupils(pupils);
        FurtherEducationDatasetHandler handler = new(repository);

        IEnumerable<Dataset> result = await handler
            .GetAvailableDatasetsAsync(new[] { "abc" });

        Assert.Contains(Dataset.SEN, result);
        Assert.DoesNotContain(Dataset.PP, result);
    }

    [Fact]
    public async Task GetAvailableDatasetsAsync_ReturnsBoth_WhenBothConditionsAreMet()
    {
        List<FurtherEducationPupil> pupils = new()
        {
            FurtherEducationPupilTestDoubles.Create(includePupilPremium: true),
            FurtherEducationPupilTestDoubles.Create(includeSen: true)
        };

        IFurtherEducationReadOnlyRepository repository = FurtherEducationRepositoryReadOnlyTestDouble.WithPupils(pupils);
        FurtherEducationDatasetHandler handler = new(repository);

        IEnumerable<Dataset> result = await handler
            .GetAvailableDatasetsAsync(new[] { "abc", "def" });

        Assert.Contains(Dataset.PP, result);
        Assert.Contains(Dataset.SEN, result);
    }

    [Fact]
    public async Task GetAvailableDatasetsAsync_ReturnsEmpty_When_NoPupilsHaveRelevantData()
    {
        List<FurtherEducationPupil> pupils = new()
        {
            FurtherEducationPupilTestDoubles.Create(includePupilPremium: false),
            FurtherEducationPupilTestDoubles.Create(includeSen: false)
        };

        IFurtherEducationReadOnlyRepository repository = FurtherEducationRepositoryReadOnlyTestDouble.WithPupils(pupils);
        FurtherEducationDatasetHandler handler = new(repository);

        IEnumerable<Dataset> result = await handler
            .GetAvailableDatasetsAsync(new[] { "abc", "def" });

        Assert.Empty(result);
    }
}
