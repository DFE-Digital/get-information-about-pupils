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
        Mock<IFurtherEducationReadOnlyRepository> mockRepository = new();
        Mock<IMapper<FurtherEducationPupil, FurtherEducationSENOutputRecord>> mockSenMapper =
            MapperTestDoubles.Default<FurtherEducationPupil, FurtherEducationSENOutputRecord>();

        Assert.Throws<ArgumentNullException>(() =>
            new FurtherEducationAggregationHandler(mockRepository.Object, null!, mockSenMapper.Object));
    }

    [Fact]
    public void Constructor_Throws_WhenSenMapperIsNull()
    {
        Mock<IFurtherEducationReadOnlyRepository> mockRepository = new();
        Mock<IMapper<FurtherEducationPupil, FurtherEducationPPOutputRecord>> mockPpMapper =
           MapperTestDoubles.Default<FurtherEducationPupil, FurtherEducationPPOutputRecord>();

        Assert.Throws<ArgumentNullException>(() =>
            new FurtherEducationAggregationHandler(mockRepository.Object, mockPpMapper.Object, null!));
    }

    [Fact]
    public void SupportedDownloadType_ReturnsFurtherEducation()
    {
        Mock<IFurtherEducationReadOnlyRepository> mockRepository = new();
        Mock<IMapper<FurtherEducationPupil, FurtherEducationPPOutputRecord>> mockPpMapper =
            MapperTestDoubles.Default<FurtherEducationPupil, FurtherEducationPPOutputRecord>();
        Mock<IMapper<FurtherEducationPupil, FurtherEducationSENOutputRecord>> mockSenMapper =
            MapperTestDoubles.Default<FurtherEducationPupil, FurtherEducationSENOutputRecord>();

        FurtherEducationAggregationHandler handler = new(
            mockRepository.Object,
            ppMapper: mockPpMapper.Object,
            senMapper: mockSenMapper.Object);

        DownloadType result = handler.SupportedDownloadType;

        Assert.Equal(DownloadType.FurtherEducation, result);
    }

    [Fact]
    public async Task AggregateAsync_AddsPP_WhenDatasetSelected_AndPupilHasPPData()
    {
        Mock<IFurtherEducationReadOnlyRepository> mockRepository = new();
        Mock<IMapper<FurtherEducationPupil, FurtherEducationPPOutputRecord>> mockPpMapper =
            MapperTestDoubles.Default<FurtherEducationPupil, FurtherEducationPPOutputRecord>();
        Mock<IMapper<FurtherEducationPupil, FurtherEducationSENOutputRecord>> mockSenMapper =
            MapperTestDoubles.Default<FurtherEducationPupil, FurtherEducationSENOutputRecord>();

        FurtherEducationPupil pupil = FurtherEducationPupilTestDoubles.Create(includePupilPremium: true);
        FurtherEducationPPOutputRecord mapped = new();

        mockRepository.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>()))
             .ReturnsAsync(new[] { pupil });

        mockPpMapper.Setup(m => m.Map(pupil)).Returns(mapped);

        FurtherEducationAggregationHandler handler = new(
            mockRepository.Object,
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
        Mock<IFurtherEducationReadOnlyRepository> mockRepository = new();
        Mock<IMapper<FurtherEducationPupil, FurtherEducationPPOutputRecord>> mockPpMapper =
           MapperTestDoubles.Default<FurtherEducationPupil, FurtherEducationPPOutputRecord>();
        Mock<IMapper<FurtherEducationPupil, FurtherEducationSENOutputRecord>> mockSenMapper =
            MapperTestDoubles.Default<FurtherEducationPupil, FurtherEducationSENOutputRecord>();

        FurtherEducationPupil pupil = FurtherEducationPupilTestDoubles.Create(includePupilPremium: false);

        mockRepository.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>()))
             .ReturnsAsync(new[] { pupil });

        FurtherEducationAggregationHandler handler = new(
            feReadRepository: mockRepository.Object,
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
        Mock<IFurtherEducationReadOnlyRepository> mockRepository = new();
        Mock<IMapper<FurtherEducationPupil, FurtherEducationPPOutputRecord>> mockPpMapper =
            MapperTestDoubles.Default<FurtherEducationPupil, FurtherEducationPPOutputRecord>();
        Mock<IMapper<FurtherEducationPupil, FurtherEducationSENOutputRecord>> mockSenMapper =
            MapperTestDoubles.Default<FurtherEducationPupil, FurtherEducationSENOutputRecord>();

        FurtherEducationPupil pupil = FurtherEducationPupilTestDoubles.Create(includePupilPremium: true);

        mockRepository.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>()))
             .ReturnsAsync(new[] { pupil });

        FurtherEducationAggregationHandler handler = new(
           feReadRepository: mockRepository.Object,
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
        Mock<IFurtherEducationReadOnlyRepository> mockRepository = new();
        Mock<IMapper<FurtherEducationPupil, FurtherEducationPPOutputRecord>> mockPpMapper =
            MapperTestDoubles.Default<FurtherEducationPupil, FurtherEducationPPOutputRecord>();
        Mock<IMapper<FurtherEducationPupil, FurtherEducationSENOutputRecord>> mockSenMapper =
            MapperTestDoubles.Default<FurtherEducationPupil, FurtherEducationSENOutputRecord>();

        FurtherEducationPupil pupil = FurtherEducationPupilTestDoubles.Create(includeSen: true);
        FurtherEducationSENOutputRecord mapped = new();

        mockRepository.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>()))
             .ReturnsAsync(new[] { pupil });

        mockSenMapper.Setup(m => m.Map(pupil)).Returns(mapped);

        FurtherEducationAggregationHandler handler = new(
            feReadRepository: mockRepository.Object,
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
        Mock<IFurtherEducationReadOnlyRepository> mockRepository = new();
        Mock<IMapper<FurtherEducationPupil, FurtherEducationPPOutputRecord>> mockPpMapper =
           MapperTestDoubles.Default<FurtherEducationPupil, FurtherEducationPPOutputRecord>();
        Mock<IMapper<FurtherEducationPupil, FurtherEducationSENOutputRecord>> mockSenMapper =
            MapperTestDoubles.Default<FurtherEducationPupil, FurtherEducationSENOutputRecord>();

        FurtherEducationPupil pupil = FurtherEducationPupilTestDoubles.Create(includeSen: false);

        mockRepository.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>()))
             .ReturnsAsync(new[] { pupil });

        FurtherEducationAggregationHandler handler = new(
            feReadRepository: mockRepository.Object,
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
        Mock<IFurtherEducationReadOnlyRepository> mockRepository = new();
        Mock<IMapper<FurtherEducationPupil, FurtherEducationPPOutputRecord>> mockPpMapper =
           MapperTestDoubles.Default<FurtherEducationPupil, FurtherEducationPPOutputRecord>();
        Mock<IMapper<FurtherEducationPupil, FurtherEducationSENOutputRecord>> mockSenMapper =
            MapperTestDoubles.Default<FurtherEducationPupil, FurtherEducationSENOutputRecord>();
        FurtherEducationPupil pupil = FurtherEducationPupilTestDoubles.Create(includeSen: true);

        mockRepository.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>()))
             .ReturnsAsync(new[] { pupil });

        FurtherEducationAggregationHandler handler = new(
            feReadRepository: mockRepository.Object,
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
        Mock<IFurtherEducationReadOnlyRepository> mockRepository = new();
        Mock<IMapper<FurtherEducationPupil, FurtherEducationPPOutputRecord>> mockPpMapper =
           MapperTestDoubles.Default<FurtherEducationPupil, FurtherEducationPPOutputRecord>();
        Mock<IMapper<FurtherEducationPupil, FurtherEducationSENOutputRecord>> mockSenMapper =
            MapperTestDoubles.Default<FurtherEducationPupil, FurtherEducationSENOutputRecord>();
        FurtherEducationPupil pupil = FurtherEducationPupilTestDoubles.Create(
            includePupilPremium: true,
            includeSen: true);

        FurtherEducationPPOutputRecord ppMapped = new();
        FurtherEducationSENOutputRecord senMapped = new();

        mockRepository.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>()))
             .ReturnsAsync(new[] { pupil });

        mockPpMapper.Setup(m => m.Map(pupil)).Returns(ppMapped);
        mockSenMapper.Setup(m => m.Map(pupil)).Returns(senMapped);

        FurtherEducationAggregationHandler handler = new(
            feReadRepository: mockRepository.Object,
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
