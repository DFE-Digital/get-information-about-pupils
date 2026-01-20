using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Downloads.Application.Aggregators;
using DfE.GIAP.Core.Downloads.Application.Aggregators.Handlers;
using DfE.GIAP.Core.Downloads.Application.Enums;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.DownloadOutputs;
using DfE.GIAP.Core.Downloads.Application.Repositories;
using DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.Downloads.Application.Aggregators.Handlers;

public sealed class FurtherEducationAggregationHandlerTests
{
    private readonly Mock<IFurtherEducationReadOnlyRepository> _repo = new();
    private readonly Mock<IMapper<FurtherEducationPupil, FurtherEducationPPOutputRecord>> _ppMapper = new();
    private readonly Mock<IMapper<FurtherEducationPupil, FurtherEducationSENOutputRecord>> _senMapper = new();

    private FurtherEducationAggregationHandler CreateHandler() =>
        new(_repo.Object, _ppMapper.Object, _senMapper.Object);

    // ------------------------------------------------------------
    // PP Behaviour
    // ------------------------------------------------------------

    [Fact]
    public async Task AggregateAsync_AddsPP_WhenDatasetSelected_AndPupilHasPPData()
    {
        FurtherEducationPupil pupil = FurtherEducationPupilTestDoubles.Create(includePupilPremium: true);
        FurtherEducationPPOutputRecord mapped = new();

        _repo.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>()))
             .ReturnsAsync(new[] { pupil });

        _ppMapper.Setup(m => m.Map(pupil)).Returns(mapped);

        FurtherEducationAggregationHandler handler = CreateHandler();

        PupilDatasetCollection result = await handler.AggregateAsync(
            new[] { "id1" },
            new[] { Dataset.FE_PP });

        Assert.Single(result.FurtherEducationPP);
        Assert.Same(mapped, result.FurtherEducationPP.First());
    }

    [Fact]
    public async Task AggregateAsync_DoesNotAddPP_WhenPupilHasNoPPData()
    {
        FurtherEducationPupil pupil = FurtherEducationPupilTestDoubles.Create(includePupilPremium: false);

        _repo.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>()))
             .ReturnsAsync(new[] { pupil });

        FurtherEducationAggregationHandler handler = CreateHandler();

        PupilDatasetCollection result = await handler.AggregateAsync(
            new[] { "id1" },
            new[] { Dataset.FE_PP });

        Assert.Empty(result.FurtherEducationPP);
        _ppMapper.Verify(m => m.Map(It.IsAny<FurtherEducationPupil>()), Times.Never);
    }

    [Fact]
    public async Task AggregateAsync_DoesNotAddPP_WhenDatasetNotSelected()
    {
        FurtherEducationPupil pupil = FurtherEducationPupilTestDoubles.Create(includePupilPremium: true);

        _repo.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>()))
             .ReturnsAsync(new[] { pupil });

        FurtherEducationAggregationHandler handler = CreateHandler();

        PupilDatasetCollection result = await handler.AggregateAsync(
            new[] { "id1" },
            Enumerable.Empty<Dataset>());

        Assert.Empty(result.FurtherEducationPP);
        _ppMapper.Verify(m => m.Map(It.IsAny<FurtherEducationPupil>()), Times.Never);
    }


    [Fact]
    public async Task AggregateAsync_AddsSEN_WhenDatasetSelected_AndPupilHasSENData()
    {
        FurtherEducationPupil pupil = FurtherEducationPupilTestDoubles.Create(includeSen: true);
        FurtherEducationSENOutputRecord mapped = new();

        _repo.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>()))
             .ReturnsAsync(new[] { pupil });

        _senMapper.Setup(m => m.Map(pupil)).Returns(mapped);

        FurtherEducationAggregationHandler handler = CreateHandler();

        PupilDatasetCollection result = await handler.AggregateAsync(
            new[] { "id1" },
            new[] { Dataset.SEN });

        Assert.Single(result.SEN);
        Assert.Same(mapped, result.SEN.First());
    }

    [Fact]
    public async Task AggregateAsync_DoesNotAddSEN_WhenPupilHasNoSENData()
    {
        FurtherEducationPupil pupil = FurtherEducationPupilTestDoubles.Create(includeSen: false);

        _repo.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>()))
             .ReturnsAsync(new[] { pupil });

        FurtherEducationAggregationHandler handler = CreateHandler();

        PupilDatasetCollection result = await handler.AggregateAsync(
            new[] { "id1" },
            new[] { Dataset.SEN });

        Assert.Empty(result.SEN);
        _senMapper.Verify(m => m.Map(It.IsAny<FurtherEducationPupil>()), Times.Never);
    }

    [Fact]
    public async Task AggregateAsync_DoesNotAddSEN_WhenDatasetNotSelected()
    {
        FurtherEducationPupil pupil = FurtherEducationPupilTestDoubles.Create(includeSen: true);

        _repo.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>()))
             .ReturnsAsync(new[] { pupil });

        FurtherEducationAggregationHandler handler = CreateHandler();

        PupilDatasetCollection result = await handler.AggregateAsync(
            new[] { "id1" },
            Enumerable.Empty<Dataset>());

        Assert.Empty(result.SEN);
        _senMapper.Verify(m => m.Map(It.IsAny<FurtherEducationPupil>()), Times.Never);
    }


    [Fact]
    public async Task AggregateAsync_AddsBothPPAndSEN_WhenBothDatasetsSelected()
    {
        FurtherEducationPupil pupil = FurtherEducationPupilTestDoubles.Create(
            includePupilPremium: true,
            includeSen: true);

        FurtherEducationPPOutputRecord ppMapped = new();
        FurtherEducationSENOutputRecord senMapped = new();

        _repo.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>()))
             .ReturnsAsync(new[] { pupil });

        _ppMapper.Setup(m => m.Map(pupil)).Returns(ppMapped);
        _senMapper.Setup(m => m.Map(pupil)).Returns(senMapped);

        FurtherEducationAggregationHandler handler = CreateHandler();

        PupilDatasetCollection result = await handler.AggregateAsync(
            new[] { "id1" },
            new[] { Dataset.FE_PP, Dataset.SEN });

        Assert.Single(result.FurtherEducationPP);
        Assert.Single(result.SEN);
    }
}
