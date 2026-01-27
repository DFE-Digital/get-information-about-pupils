using DfE.GIAP.Core.Downloads.Application.Aggregators;
using DfE.GIAP.Core.Downloads.Application.Aggregators.Handlers;
using DfE.GIAP.Core.Downloads.Application.Enums;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.DownloadOutputs;
using DfE.GIAP.Core.Downloads.Application.Repositories;
using DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;
using Moq;

namespace DfE.GIAP.Core.UnitTests.Downloads.Application.Aggregators.Handlers;

public sealed class PupilPremiumAggregationHandlerTests
{
    [Fact]
    public void Constructor_Throws_WhenRepositoryIsNull()
    {
        Mock<IMapper<PupilPremiumPupil, IEnumerable<PupilPremiumOutputRecord>>> mockMapper =
            new();

        Assert.Throws<ArgumentNullException>(() =>
            new PupilPremiumAggregationHandler(null!, mockMapper.Object));
    }

    [Fact]
    public void Constructor_Throws_WhenMapperIsNull()
    {
        Mock<IPupilPremiumReadOnlyRepository> mockRepository =
            new();

        Assert.Throws<ArgumentNullException>(() =>
            new PupilPremiumAggregationHandler(mockRepository.Object, null!));
    }

    [Fact]
    public void SupportedDownloadType_ReturnsPupilPremium()
    {
        Mock<IPupilPremiumReadOnlyRepository> mockRepository =
            new();

        Mock<IMapper<PupilPremiumPupil, IEnumerable<PupilPremiumOutputRecord>>> mockMapper =
            new();

        PupilPremiumAggregationHandler handler =
            new(mockRepository.Object, mockMapper.Object);

        DownloadType result = handler.SupportedDownloadType;

        Assert.Equal(DownloadType.PupilPremium, result);
    }

    [Fact]
    public async Task AggregateAsync_AddsMappedRecords_ForEachPupilWithPPData()
    {
        Mock<IPupilPremiumReadOnlyRepository> mockRepository =
            new();

        Mock<IMapper<PupilPremiumPupil, IEnumerable<PupilPremiumOutputRecord>>> mockMapper =
            new();

        PupilPremiumPupil pupil1 = PupilPremiumPupilTestDouble.Create(includePupilPremium: true);
        PupilPremiumPupil pupil2 = PupilPremiumPupilTestDouble.Create(includePupilPremium: true);

        IEnumerable<PupilPremiumOutputRecord> mapped1 =
            new[] { new PupilPremiumOutputRecord() };

        IEnumerable<PupilPremiumOutputRecord> mapped2 =
            new[]
            {
                new PupilPremiumOutputRecord(),
                new PupilPremiumOutputRecord()
            };

        mockRepository.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new[] { pupil1, pupil2 });

        mockMapper.Setup(m => m.Map(pupil1)).Returns(mapped1);
        mockMapper.Setup(m => m.Map(pupil2)).Returns(mapped2);

        PupilPremiumAggregationHandler handler =
            new(mockRepository.Object, mockMapper.Object);

        PupilDatasetCollection result = await handler.AggregateAsync(
            new[] { "id1" },
            new[] { Dataset.PP });

        Assert.Equal(3, result.PupilPremium.Count);
        mockMapper.Verify(m => m.Map(pupil1), Times.Once);
        mockMapper.Verify(m => m.Map(pupil2), Times.Once);
    }

    [Fact]
    public async Task AggregateAsync_DoesNotAddRecords_WhenPupilHasNoPPData()
    {
        Mock<IPupilPremiumReadOnlyRepository> mockRepository =
            new();

        Mock<IMapper<PupilPremiumPupil, IEnumerable<PupilPremiumOutputRecord>>> mockMapper =
            new();

        PupilPremiumPupil pupil = PupilPremiumPupilTestDouble.Create(includePupilPremium: false);

        mockRepository.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new[] { pupil });

        PupilPremiumAggregationHandler handler =
            new(mockRepository.Object, mockMapper.Object);

        PupilDatasetCollection result = await handler.AggregateAsync(
            new[] { "id1" },
            new[] { Dataset.PP });

        Assert.Empty(result.PupilPremium);
        mockMapper.Verify(m => m.Map(It.IsAny<PupilPremiumPupil>()), Times.Never);
    }

    [Fact]
    public async Task AggregateAsync_DoesNotAddRecords_WhenDatasetNotSelected()
    {
        Mock<IPupilPremiumReadOnlyRepository> mockRepository =
            new();

        Mock<IMapper<PupilPremiumPupil, IEnumerable<PupilPremiumOutputRecord>>> mockMapper =
            new();

        PupilPremiumPupil pupil = PupilPremiumPupilTestDouble.Create(includePupilPremium: true);

        mockRepository.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new[] { pupil });

        PupilPremiumAggregationHandler handler =
            new(mockRepository.Object, mockMapper.Object);

        PupilDatasetCollection result = await handler.AggregateAsync(
            new[] { "id1" },
            Enumerable.Empty<Dataset>());

        Assert.Empty(result.PupilPremium);
        mockMapper.Verify(m => m.Map(It.IsAny<PupilPremiumPupil>()), Times.Never);
    }

    [Fact]
    public async Task AggregateAsync_HandlesEmptyRepositoryResult()
    {
        Mock<IPupilPremiumReadOnlyRepository> mockRepository =
            new();

        Mock<IMapper<PupilPremiumPupil, IEnumerable<PupilPremiumOutputRecord>>> mockMapper =
            new();

        mockRepository.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(Array.Empty<PupilPremiumPupil>());

        PupilPremiumAggregationHandler handler =
            new(mockRepository.Object, mockMapper.Object);

        PupilDatasetCollection result = await handler.AggregateAsync(
            new[] { "id1" },
            new[] { Dataset.PP });

        Assert.Empty(result.PupilPremium);
        mockMapper.Verify(m => m.Map(It.IsAny<PupilPremiumPupil>()), Times.Never);
    }
}
