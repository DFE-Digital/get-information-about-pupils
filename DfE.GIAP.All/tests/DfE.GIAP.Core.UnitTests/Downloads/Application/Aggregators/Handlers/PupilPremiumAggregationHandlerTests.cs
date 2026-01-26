using DfE.GIAP.Core.Downloads.Application.Aggregators;
using DfE.GIAP.Core.Downloads.Application.Aggregators.Handlers;
using DfE.GIAP.Core.Downloads.Application.Enums;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.DownloadOutputs;
using DfE.GIAP.Core.Downloads.Application.Repositories;
using DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;
using DfE.GIAP.SharedTests.Common;

namespace DfE.GIAP.Core.UnitTests.Downloads.Application.Aggregators.Handlers;

public sealed class PupilPremiumAggregationHandlerTests
{
    [Fact]
    public void Constructor_Throws_WhenRepositoryIsNull()
    {
        Mock<IMapper<PupilPremiumPupil, PupilPremiumOutputRecord>> mockPpMapper =
            MapperTestDoubles.Default<PupilPremiumPupil, PupilPremiumOutputRecord>();

        Assert.Throws<ArgumentNullException>(() =>
            new PupilPremiumAggregationHandler(null!, mockPpMapper.Object));
    }

    [Fact]
    public void Constructor_Throws_WhenPpMapperIsNull()
    {
        Mock<IPupilPremiumReadOnlyRepository> mockRepository = new();
        Mock<IMapper<PupilPremiumPupil, PupilPremiumOutputRecord>> mockPpMapper =
           MapperTestDoubles.Default<PupilPremiumPupil, PupilPremiumOutputRecord>();

        Assert.Throws<ArgumentNullException>(() =>
            new PupilPremiumAggregationHandler(mockRepository.Object, null!));
    }

    [Fact]
    public void SupportedDownloadType_ReturnsPupilPremium()
    {
        Mock<IPupilPremiumReadOnlyRepository> mockRepository = new();
        Mock<IMapper<PupilPremiumPupil, PupilPremiumOutputRecord>> mockPpMapper =
          MapperTestDoubles.Default<PupilPremiumPupil, PupilPremiumOutputRecord>();

        PupilPremiumAggregationHandler handler = new(
            mockRepository.Object,
            ppMapper: mockPpMapper.Object);

        DownloadType result = handler.SupportedDownloadType;

        Assert.Equal(DownloadType.PupilPremium, result);
    }

    [Fact]
    public async Task AggregateAsync_AddsPP_WhenDatasetSelected_AndPupilHasPPData()
    {
        Mock<IPupilPremiumReadOnlyRepository> mockRepository = new();
        Mock<IMapper<PupilPremiumPupil, PupilPremiumOutputRecord>> mockPpMapper =
          MapperTestDoubles.Default<PupilPremiumPupil, PupilPremiumOutputRecord>();

        PupilPremiumPupil pupil = PupilPremiumPupilTestDouble.Create(includePupilPremium: true);
        PupilPremiumOutputRecord mapped = new();

        mockRepository.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>()))
             .ReturnsAsync(new[] { pupil });

        mockPpMapper.Setup(m => m.Map(pupil)).Returns(mapped);

        PupilPremiumAggregationHandler handler = new(
            mockRepository.Object,
            ppMapper: mockPpMapper.Object);

        PupilDatasetCollection result = await handler.AggregateAsync(
            new[] { "id1" },
            new[] { Dataset.PP });

        Assert.Single(result.PupilPremium);
        mockPpMapper.Verify(m => m.Map(pupil), Times.Once);
    }

    [Fact]
    public async Task AggregateAsync_DoesNotAddPP_WhenPupilHasNoPPData()
    {
        Mock<IPupilPremiumReadOnlyRepository> mockRepository = new();
        Mock<IMapper<PupilPremiumPupil, PupilPremiumOutputRecord>> mockPpMapper =
          MapperTestDoubles.Default<PupilPremiumPupil, PupilPremiumOutputRecord>();

        PupilPremiumPupil pupil = PupilPremiumPupilTestDouble.Create(includePupilPremium: false);
        PupilPremiumOutputRecord mapped = new();

        mockRepository.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>()))
             .ReturnsAsync(new[] { pupil });

        PupilPremiumAggregationHandler handler = new(
            mockRepository.Object,
            ppMapper: mockPpMapper.Object);

        PupilDatasetCollection result = await handler.AggregateAsync(
            new[] { "id1" },
            new[] { Dataset.PP });

        Assert.Empty(result.PupilPremium);
        mockPpMapper.Verify(m => m.Map(It.IsAny<PupilPremiumPupil>()), Times.Never);
    }

    [Fact]
    public async Task AggregateAsync_DoesNotAddPP_WhenDatasetNotSelected()
    {
        Mock<IPupilPremiumReadOnlyRepository> mockRepository = new();
        Mock<IMapper<PupilPremiumPupil, PupilPremiumOutputRecord>> mockPpMapper =
          MapperTestDoubles.Default<PupilPremiumPupil, PupilPremiumOutputRecord>();

        PupilPremiumPupil pupil = PupilPremiumPupilTestDouble.Create(includePupilPremium: true);
        PupilPremiumOutputRecord mapped = new();

        mockRepository.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>()))
             .ReturnsAsync(new[] { pupil });

        PupilPremiumAggregationHandler handler = new(
            mockRepository.Object,
            ppMapper: mockPpMapper.Object);

        PupilDatasetCollection result = await handler.AggregateAsync(
            new[] { "id1" },
            Enumerable.Empty<Dataset>());

        Assert.Empty(result.PupilPremium);
        mockPpMapper.Verify(m => m.Map(It.IsAny<PupilPremiumPupil>()), Times.Never);
    }
}
