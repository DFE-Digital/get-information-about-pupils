using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Downloads.Application.Aggregators;
using DfE.GIAP.Core.Downloads.Application.Aggregators.Handlers;
using DfE.GIAP.Core.Downloads.Application.Enums;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.DownloadOutputs;
using DfE.GIAP.Core.Downloads.Application.Repositories;
using DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;
using DfE.GIAP.SharedTests.Common;

namespace DfE.GIAP.Core.UnitTests.Downloads.Application.Aggregators.Handlers;

public sealed class FurtherEducationAggregationHandlerTests
{
    private readonly Mock<IFurtherEducationReadOnlyRepository> _repo = new();

    [Fact]
    public void Constructor_Throws_WhenRepositoryIsNull()
    {
        Mock<IMapper<FurtherEducationPupil, FurtherEducationPPOutputRecord>> mockPpMapper =
            MapperTestDoubles.Default<FurtherEducationPupil, FurtherEducationPPOutputRecord>();
        Mock<IMapper<FurtherEducationPupil, FurtherEducationSENOutputRecord>> mockSenMapper =
            MapperTestDoubles.Default<FurtherEducationPupil, FurtherEducationSENOutputRecord>();

        Assert.Throws<ArgumentNullException>(() =>
            new FurtherEducationAggregationHandler(null!, mockPpMapper.Object, mockSenMapper.Object));
    }

    [Fact]
    public void Constructor_Throws_WhenPpMapperIsNull()
    {
        Mock<IMapper<FurtherEducationPupil, FurtherEducationSENOutputRecord>> mockSenMapper =
            MapperTestDoubles.Default<FurtherEducationPupil, FurtherEducationSENOutputRecord>();

        Assert.Throws<ArgumentNullException>(() =>
            new FurtherEducationAggregationHandler(_repo.Object, null!, mockSenMapper.Object));
    }

    [Fact]
    public void Constructor_Throws_WhenSenMapperIsNull()
    {
        Mock<IMapper<FurtherEducationPupil, FurtherEducationPPOutputRecord>> mockPpMapper =
           MapperTestDoubles.Default<FurtherEducationPupil, FurtherEducationPPOutputRecord>();

        Assert.Throws<ArgumentNullException>(() =>
            new FurtherEducationAggregationHandler(_repo.Object, mockPpMapper.Object, null!));
    }

    [Fact]
    public async Task AggregateAsync_AddsPP_WhenDatasetSelected_AndPupilHasPPData()
    {
        Mock<IMapper<FurtherEducationPupil, FurtherEducationPPOutputRecord>> mockPpMapper =
            MapperTestDoubles.Default<FurtherEducationPupil, FurtherEducationPPOutputRecord>();
        Mock<IMapper<FurtherEducationPupil, FurtherEducationSENOutputRecord>> mockSenMapper =
            MapperTestDoubles.Default<FurtherEducationPupil, FurtherEducationSENOutputRecord>();

        FurtherEducationPupil pupil = FurtherEducationPupilTestDoubles.Create(includePupilPremium: true);
        FurtherEducationPPOutputRecord mapped = new();

        _repo.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>()))
             .ReturnsAsync(new[] { pupil });

        mockPpMapper.Setup(m => m.Map(pupil)).Returns(mapped);

        FurtherEducationAggregationHandler handler = new(
            _repo.Object,
            ppMapper: mockPpMapper.Object,
            senMapper: mockSenMapper.Object);

        PupilDatasetCollection result = await handler.AggregateAsync(
            new[] { "id1" },
            new[] { Dataset.FE_PP });

        Assert.Single(result.FurtherEducationPP);
        mockPpMapper.Verify(m => m.Map(pupil), Times.Once);
    }

    [Fact]
    public async Task AggregateAsync_DoesNotAddPP_WhenPupilHasNoPPData()
    {
        Mock<IMapper<FurtherEducationPupil, FurtherEducationPPOutputRecord>> mockPpMapper =
           MapperTestDoubles.Default<FurtherEducationPupil, FurtherEducationPPOutputRecord>();
        Mock<IMapper<FurtherEducationPupil, FurtherEducationSENOutputRecord>> mockSenMapper =
            MapperTestDoubles.Default<FurtherEducationPupil, FurtherEducationSENOutputRecord>();

        FurtherEducationPupil pupil = FurtherEducationPupilTestDoubles.Create(includePupilPremium: false);

        _repo.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>()))
             .ReturnsAsync(new[] { pupil });

        FurtherEducationAggregationHandler handler = new(
            feReadRepository: _repo.Object,
            ppMapper: mockPpMapper.Object,
            senMapper: mockSenMapper.Object);

        PupilDatasetCollection result = await handler.AggregateAsync(
            new[] { "id1" },
            new[] { Dataset.FE_PP });

        Assert.Empty(result.FurtherEducationPP);
        mockPpMapper.Verify(m => m.Map(It.IsAny<FurtherEducationPupil>()), Times.Never);
    }

    [Fact]
    public async Task AggregateAsync_DoesNotAddPP_WhenDatasetNotSelected()
    {
        Mock<IMapper<FurtherEducationPupil, FurtherEducationPPOutputRecord>> mockPpMapper =
            MapperTestDoubles.Default<FurtherEducationPupil, FurtherEducationPPOutputRecord>();
        Mock<IMapper<FurtherEducationPupil, FurtherEducationSENOutputRecord>> mockSenMapper =
            MapperTestDoubles.Default<FurtherEducationPupil, FurtherEducationSENOutputRecord>();

        FurtherEducationPupil pupil = FurtherEducationPupilTestDoubles.Create(includePupilPremium: true);

        _repo.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>()))
             .ReturnsAsync(new[] { pupil });

        FurtherEducationAggregationHandler handler = new(
           feReadRepository: _repo.Object,
           ppMapper: mockPpMapper.Object,
           senMapper: mockSenMapper.Object);

        PupilDatasetCollection result = await handler.AggregateAsync(
            new[] { "id1" },
            Enumerable.Empty<Dataset>());

        Assert.Empty(result.FurtherEducationPP);
        mockPpMapper.Verify(m => m.Map(It.IsAny<FurtherEducationPupil>()), Times.Never);
    }


    [Fact]
    public async Task AggregateAsync_AddsSEN_WhenDatasetSelected_AndPupilHasSENData()
    {
        Mock<IMapper<FurtherEducationPupil, FurtherEducationPPOutputRecord>> mockPpMapper =
            MapperTestDoubles.Default<FurtherEducationPupil, FurtherEducationPPOutputRecord>();
        Mock<IMapper<FurtherEducationPupil, FurtherEducationSENOutputRecord>> mockSenMapper =
            MapperTestDoubles.Default<FurtherEducationPupil, FurtherEducationSENOutputRecord>();

        FurtherEducationPupil pupil = FurtherEducationPupilTestDoubles.Create(includeSen: true);
        FurtherEducationSENOutputRecord mapped = new();

        _repo.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>()))
             .ReturnsAsync(new[] { pupil });

        mockSenMapper.Setup(m => m.Map(pupil)).Returns(mapped);

        FurtherEducationAggregationHandler handler = new(
            feReadRepository: _repo.Object,
            ppMapper: mockPpMapper.Object,
            senMapper: mockSenMapper.Object);

        PupilDatasetCollection result = await handler.AggregateAsync(
            new[] { "id1" },
            new[] { Dataset.SEN });

        Assert.Single(result.SEN);
        mockSenMapper.Verify(m => m.Map(pupil), Times.Once);
    }

    [Fact]
    public async Task AggregateAsync_DoesNotAddSEN_WhenPupilHasNoSENData()
    {
        Mock<IMapper<FurtherEducationPupil, FurtherEducationPPOutputRecord>> mockPpMapper =
           MapperTestDoubles.Default<FurtherEducationPupil, FurtherEducationPPOutputRecord>();
        Mock<IMapper<FurtherEducationPupil, FurtherEducationSENOutputRecord>> mockSenMapper =
            MapperTestDoubles.Default<FurtherEducationPupil, FurtherEducationSENOutputRecord>();

        FurtherEducationPupil pupil = FurtherEducationPupilTestDoubles.Create(includeSen: false);

        _repo.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>()))
             .ReturnsAsync(new[] { pupil });

        FurtherEducationAggregationHandler handler = new(
            feReadRepository: _repo.Object,
            ppMapper: mockPpMapper.Object,
            senMapper: mockSenMapper.Object);

        PupilDatasetCollection result = await handler.AggregateAsync(
            new[] { "id1" },
            new[] { Dataset.SEN });

        Assert.Empty(result.SEN);
        mockSenMapper.Verify(m => m.Map(It.IsAny<FurtherEducationPupil>()), Times.Never);
    }

    [Fact]
    public async Task AggregateAsync_DoesNotAddSEN_WhenDatasetNotSelected()
    {
        Mock<IMapper<FurtherEducationPupil, FurtherEducationPPOutputRecord>> mockPpMapper =
           MapperTestDoubles.Default<FurtherEducationPupil, FurtherEducationPPOutputRecord>();
        Mock<IMapper<FurtherEducationPupil, FurtherEducationSENOutputRecord>> mockSenMapper =
            MapperTestDoubles.Default<FurtherEducationPupil, FurtherEducationSENOutputRecord>();
        FurtherEducationPupil pupil = FurtherEducationPupilTestDoubles.Create(includeSen: true);

        _repo.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>()))
             .ReturnsAsync(new[] { pupil });

        FurtherEducationAggregationHandler handler = new(
            feReadRepository: _repo.Object,
            ppMapper: mockPpMapper.Object,
            senMapper: mockSenMapper.Object);

        PupilDatasetCollection result = await handler.AggregateAsync(
            new[] { "id1" },
            Enumerable.Empty<Dataset>());

        Assert.Empty(result.SEN);
        mockSenMapper.Verify(m => m.Map(It.IsAny<FurtherEducationPupil>()), Times.Never);
    }


    [Fact]
    public async Task AggregateAsync_AddsBothPPAndSEN_WhenBothDatasetsSelected()
    {
        Mock<IMapper<FurtherEducationPupil, FurtherEducationPPOutputRecord>> mockPpMapper =
           MapperTestDoubles.Default<FurtherEducationPupil, FurtherEducationPPOutputRecord>();
        Mock<IMapper<FurtherEducationPupil, FurtherEducationSENOutputRecord>> mockSenMapper =
            MapperTestDoubles.Default<FurtherEducationPupil, FurtherEducationSENOutputRecord>();
        FurtherEducationPupil pupil = FurtherEducationPupilTestDoubles.Create(
            includePupilPremium: true,
            includeSen: true);

        FurtherEducationPPOutputRecord ppMapped = new();
        FurtherEducationSENOutputRecord senMapped = new();

        _repo.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>()))
             .ReturnsAsync(new[] { pupil });

        mockPpMapper.Setup(m => m.Map(pupil)).Returns(ppMapped);
        mockSenMapper.Setup(m => m.Map(pupil)).Returns(senMapped);

        FurtherEducationAggregationHandler handler = new(
            feReadRepository: _repo.Object,
            ppMapper: mockPpMapper.Object,
            senMapper: mockSenMapper.Object);

        PupilDatasetCollection result = await handler.AggregateAsync(
            new[] { "id1" },
            new[] { Dataset.FE_PP, Dataset.SEN });

        Assert.Single(result.FurtherEducationPP);
        Assert.Single(result.SEN);
        mockPpMapper.Verify(m => m.Map(pupil), Times.Once);
        mockSenMapper.Verify(m => m.Map(pupil), Times.Once);
    }
}
