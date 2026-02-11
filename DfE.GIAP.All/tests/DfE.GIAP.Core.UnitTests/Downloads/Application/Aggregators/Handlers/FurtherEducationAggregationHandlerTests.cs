using DfE.GIAP.Core.Downloads.Application.DataDownloads.Aggregators;
using DfE.GIAP.Core.Downloads.Application.DataDownloads.Aggregators.Handlers;
using DfE.GIAP.Core.Downloads.Application.DataDownloads.DownloadOutputs;
using DfE.GIAP.Core.Downloads.Application.Enums;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Repositories;
using DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.Downloads.Application.Aggregators.Handlers;

public sealed class FurtherEducationAggregationHandlerTests
{
    [Fact]
    public void Constructor_Throws_WhenRepositoryIsNull()
    {
        Mock<IMapper<FurtherEducationPupil, IEnumerable<FurtherEducationPPOutputRecord>>> mockPpMapper =
            new();

        Mock<IMapper<FurtherEducationPupil, IEnumerable<FurtherEducationSENOutputRecord>>> mockSenMapper =
            new();

        Assert.Throws<ArgumentNullException>(() =>
            new FurtherEducationAggregationHandler(null!, mockPpMapper.Object, mockSenMapper.Object));
    }

    [Fact]
    public void Constructor_Throws_WhenPpMapperIsNull()
    {
        Mock<IFurtherEducationReadOnlyRepository> mockRepository =
            new();

        Mock<IMapper<FurtherEducationPupil, IEnumerable<FurtherEducationSENOutputRecord>>> mockSenMapper =
            new();

        Assert.Throws<ArgumentNullException>(() =>
            new FurtherEducationAggregationHandler(mockRepository.Object, null!, mockSenMapper.Object));
    }

    [Fact]
    public void Constructor_Throws_WhenSenMapperIsNull()
    {
        Mock<IFurtherEducationReadOnlyRepository> mockRepository =
            new();

        Mock<IMapper<FurtherEducationPupil, IEnumerable<FurtherEducationPPOutputRecord>>> mockPpMapper =
            new();

        Assert.Throws<ArgumentNullException>(() =>
            new FurtherEducationAggregationHandler(mockRepository.Object, mockPpMapper.Object, null!));
    }

    [Fact]
    public void SupportedDownloadType_ReturnsFurtherEducation()
    {
        Mock<IFurtherEducationReadOnlyRepository> mockRepository =
            new();

        Mock<IMapper<FurtherEducationPupil, IEnumerable<FurtherEducationPPOutputRecord>>> mockPpMapper =
            new();

        Mock<IMapper<FurtherEducationPupil, IEnumerable<FurtherEducationSENOutputRecord>>> mockSenMapper =
            new();

        FurtherEducationAggregationHandler handler =
            new(mockRepository.Object, mockPpMapper.Object, mockSenMapper.Object);

        Assert.Equal(DownloadType.FurtherEducation, handler.SupportedDownloadType);
    }

    [Fact]
    public async Task AggregateAsync_AddsPPRecords_WhenDatasetSelected_AndPupilHasPPData()
    {
        Mock<IFurtherEducationReadOnlyRepository> mockRepository =
            new();

        Mock<IMapper<FurtherEducationPupil, IEnumerable<FurtherEducationPPOutputRecord>>> mockPpMapper =
            new();

        Mock<IMapper<FurtherEducationPupil, IEnumerable<FurtherEducationSENOutputRecord>>> mockSenMapper =
            new();

        FurtherEducationPupil pupil = FurtherEducationPupilTestDoubles.Create(includePupilPremium: true, includeSen: false);

        IEnumerable<FurtherEducationPPOutputRecord> mapped =
            new[] { new FurtherEducationPPOutputRecord() };

        mockRepository.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new[] { pupil });

        mockPpMapper.Setup(m => m.Map(pupil)).Returns(mapped);

        FurtherEducationAggregationHandler handler =
            new(mockRepository.Object, mockPpMapper.Object, mockSenMapper.Object);

        PupilDatasetCollection result = await handler.AggregateAsync(
            new[] { "id1" },
            new[] { Dataset.FE_PP });

        Assert.Single(result.FurtherEducationPP);
        mockPpMapper.Verify(m => m.Map(pupil), Times.Once);
        mockSenMapper.Verify(m => m.Map(It.IsAny<FurtherEducationPupil>()), Times.Never);
    }

    [Fact]
    public async Task AggregateAsync_AddsSENRecords_WhenDatasetSelected_AndPupilHasSENData()
    {
        Mock<IFurtherEducationReadOnlyRepository> mockRepository =
            new();

        Mock<IMapper<FurtherEducationPupil, IEnumerable<FurtherEducationPPOutputRecord>>> mockPpMapper =
            new();

        Mock<IMapper<FurtherEducationPupil, IEnumerable<FurtherEducationSENOutputRecord>>> mockSenMapper =
            new();

        FurtherEducationPupil pupil = FurtherEducationPupilTestDoubles.Create(includePupilPremium: false, includeSen: true);

        IEnumerable<FurtherEducationSENOutputRecord> mapped =
            new[] { new FurtherEducationSENOutputRecord() };

        mockRepository.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new[] { pupil });

        mockSenMapper.Setup(m => m.Map(pupil)).Returns(mapped);

        FurtherEducationAggregationHandler handler =
            new(mockRepository.Object, mockPpMapper.Object, mockSenMapper.Object);

        PupilDatasetCollection result = await handler.AggregateAsync(
            new[] { "id1" },
            new[] { Dataset.SEN });

        Assert.Single(result.SEN);
        mockSenMapper.Verify(m => m.Map(pupil), Times.Once);
        mockPpMapper.Verify(m => m.Map(It.IsAny<FurtherEducationPupil>()), Times.Never);
    }

    [Fact]
    public async Task AggregateAsync_AddsBothPPAndSEN_WhenBothDatasetsSelected()
    {
        Mock<IFurtherEducationReadOnlyRepository> mockRepository =
            new();

        Mock<IMapper<FurtherEducationPupil, IEnumerable<FurtherEducationPPOutputRecord>>> mockPpMapper =
            new();

        Mock<IMapper<FurtherEducationPupil, IEnumerable<FurtherEducationSENOutputRecord>>> mockSenMapper =
            new();

        FurtherEducationPupil pupil = FurtherEducationPupilTestDoubles.Create(includePupilPremium: true, includeSen: true);

        IEnumerable<FurtherEducationPPOutputRecord> mappedPP =
            new[] { new FurtherEducationPPOutputRecord() };

        IEnumerable<FurtherEducationSENOutputRecord> mappedSEN =
            new[] { new FurtherEducationSENOutputRecord() };

        mockRepository.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new[] { pupil });

        mockPpMapper.Setup(m => m.Map(pupil)).Returns(mappedPP);
        mockSenMapper.Setup(m => m.Map(pupil)).Returns(mappedSEN);

        FurtherEducationAggregationHandler handler =
            new(mockRepository.Object, mockPpMapper.Object, mockSenMapper.Object);

        PupilDatasetCollection result = await handler.AggregateAsync(
            new[] { "id1" },
            new[] { Dataset.FE_PP, Dataset.SEN });

        Assert.Single(result.FurtherEducationPP);
        Assert.Single(result.SEN);

        mockPpMapper.Verify(m => m.Map(pupil), Times.Once);
        mockSenMapper.Verify(m => m.Map(pupil), Times.Once);
    }

    [Fact]
    public async Task AggregateAsync_DoesNothing_WhenDatasetNotSelected()
    {
        Mock<IFurtherEducationReadOnlyRepository> mockRepository =
            new();

        Mock<IMapper<FurtherEducationPupil, IEnumerable<FurtherEducationPPOutputRecord>>> mockPpMapper =
            new();

        Mock<IMapper<FurtherEducationPupil, IEnumerable<FurtherEducationSENOutputRecord>>> mockSenMapper =
            new();

        FurtherEducationPupil pupil = FurtherEducationPupilTestDoubles.Create(includePupilPremium: true, includeSen: true);

        mockRepository.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new[] { pupil });

        FurtherEducationAggregationHandler handler =
            new(mockRepository.Object, mockPpMapper.Object, mockSenMapper.Object);

        PupilDatasetCollection result = await handler.AggregateAsync(
            new[] { "id1" },
            Enumerable.Empty<Dataset>());

        Assert.Empty(result.FurtherEducationPP);
        Assert.Empty(result.SEN);

        mockPpMapper.Verify(m => m.Map(It.IsAny<FurtherEducationPupil>()), Times.Never);
        mockSenMapper.Verify(m => m.Map(It.IsAny<FurtherEducationPupil>()), Times.Never);
    }

    [Fact]
    public async Task AggregateAsync_HandlesEmptyRepositoryResult()
    {
        Mock<IFurtherEducationReadOnlyRepository> mockRepository =
            new();

        Mock<IMapper<FurtherEducationPupil, IEnumerable<FurtherEducationPPOutputRecord>>> mockPpMapper =
            new();

        Mock<IMapper<FurtherEducationPupil, IEnumerable<FurtherEducationSENOutputRecord>>> mockSenMapper =
            new();

        mockRepository.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(Array.Empty<FurtherEducationPupil>());

        FurtherEducationAggregationHandler handler =
            new(mockRepository.Object, mockPpMapper.Object, mockSenMapper.Object);

        PupilDatasetCollection result = await handler.AggregateAsync(
            new[] { "id1" },
            new[] { Dataset.FE_PP, Dataset.SEN });

        Assert.Empty(result.FurtherEducationPP);
        Assert.Empty(result.SEN);

        mockPpMapper.Verify(m => m.Map(It.IsAny<FurtherEducationPupil>()), Times.Never);
        mockSenMapper.Verify(m => m.Map(It.IsAny<FurtherEducationPupil>()), Times.Never);
    }
}
