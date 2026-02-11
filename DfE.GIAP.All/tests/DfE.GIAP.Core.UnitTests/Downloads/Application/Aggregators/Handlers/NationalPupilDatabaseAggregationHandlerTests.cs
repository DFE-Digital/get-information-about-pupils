using DfE.GIAP.Core.Downloads.Application.DataDownloads.Aggregators;
using DfE.GIAP.Core.Downloads.Application.DataDownloads.Aggregators.Handlers;
using DfE.GIAP.Core.Downloads.Application.DataDownloads.DownloadOutputs;
using DfE.GIAP.Core.Downloads.Application.Enums;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.Entries;
using DfE.GIAP.Core.Downloads.Application.Repositories;
using Moq;

namespace DfE.GIAP.Core.UnitTests.Downloads.Application.Aggregators.Handlers;

public sealed class NationalPupilDatabaseAggregationHandlerTests
{
    [Fact]
    public void Constructor_Throws_WhenRepositoryIsNull()
    {
        Mock<IMapper<NationalPupil, IEnumerable<CensusAutumnOutputRecord>>> autumn = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSummerOutputRecord>>> summer = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSpringOutputRecord>>> spring = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS1OutputRecord>>> ks1 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS2OutputRecord>>> ks2 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS4OutputRecord>>> ks4 = new();
        Mock<IMapper<NationalPupil, IEnumerable<MTCOutputRecord>>> mtc = new();
        Mock<IMapper<NationalPupil, IEnumerable<PhonicsOutputRecord>>> phonics = new();
        Mock<IMapper<NationalPupil, IEnumerable<EYFSPOutputRecord>>> eyfsp = new();

        Assert.Throws<ArgumentNullException>(() =>
            new NationalPupilDatabaseAggregationHandler(
                null!,
                autumn.Object,
                summer.Object,
                spring.Object,
                ks1.Object,
                ks2.Object,
                ks4.Object,
                mtc.Object,
                phonics.Object,
                eyfsp.Object));
    }

    [Fact]
    public void Constructor_Throws_WhenAutumnMapperIsNull()
    {
        Mock<INationalPupilReadOnlyRepository> repo = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSummerOutputRecord>>> summer = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSpringOutputRecord>>> spring = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS1OutputRecord>>> ks1 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS2OutputRecord>>> ks2 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS4OutputRecord>>> ks4 = new();
        Mock<IMapper<NationalPupil, IEnumerable<MTCOutputRecord>>> mtc = new();
        Mock<IMapper<NationalPupil, IEnumerable<PhonicsOutputRecord>>> phonics = new();
        Mock<IMapper<NationalPupil, IEnumerable<EYFSPOutputRecord>>> eyfsp = new();

        Assert.Throws<ArgumentNullException>(() =>
            new NationalPupilDatabaseAggregationHandler(
                repo.Object,
                null!,
                summer.Object,
                spring.Object,
                ks1.Object,
                ks2.Object,
                ks4.Object,
                mtc.Object,
                phonics.Object,
                eyfsp.Object));
    }

    [Fact]
    public void Constructor_Throws_WhenSummerMapperIsNull()
    {
        Mock<INationalPupilReadOnlyRepository> repo = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusAutumnOutputRecord>>> autumn = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSpringOutputRecord>>> spring = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS1OutputRecord>>> ks1 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS2OutputRecord>>> ks2 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS4OutputRecord>>> ks4 = new();
        Mock<IMapper<NationalPupil, IEnumerable<MTCOutputRecord>>> mtc = new();
        Mock<IMapper<NationalPupil, IEnumerable<PhonicsOutputRecord>>> phonics = new();
        Mock<IMapper<NationalPupil, IEnumerable<EYFSPOutputRecord>>> eyfsp = new();

        Assert.Throws<ArgumentNullException>(() =>
            new NationalPupilDatabaseAggregationHandler(
                repo.Object,
                autumn.Object,
                null!,
                spring.Object,
                ks1.Object,
                ks2.Object,
                ks4.Object,
                mtc.Object,
                phonics.Object,
                eyfsp.Object));
    }

    [Fact]
    public void Constructor_Throws_WhenSpringMapperIsNull()
    {
        Mock<INationalPupilReadOnlyRepository> repo = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusAutumnOutputRecord>>> autumn = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSummerOutputRecord>>> summer = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS1OutputRecord>>> ks1 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS2OutputRecord>>> ks2 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS4OutputRecord>>> ks4 = new();
        Mock<IMapper<NationalPupil, IEnumerable<MTCOutputRecord>>> mtc = new();
        Mock<IMapper<NationalPupil, IEnumerable<PhonicsOutputRecord>>> phonics = new();
        Mock<IMapper<NationalPupil, IEnumerable<EYFSPOutputRecord>>> eyfsp = new();

        Assert.Throws<ArgumentNullException>(() =>
            new NationalPupilDatabaseAggregationHandler(
                repo.Object,
                autumn.Object,
                summer.Object,
                null!,
                ks1.Object,
                ks2.Object,
                ks4.Object,
                mtc.Object,
                phonics.Object,
                eyfsp.Object));
    }

    [Fact]
    public void Constructor_Throws_WhenKs1MapperIsNull()
    {
        Mock<INationalPupilReadOnlyRepository> repo = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusAutumnOutputRecord>>> autumn = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSummerOutputRecord>>> summer = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSpringOutputRecord>>> spring = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS2OutputRecord>>> ks2 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS4OutputRecord>>> ks4 = new();
        Mock<IMapper<NationalPupil, IEnumerable<MTCOutputRecord>>> mtc = new();
        Mock<IMapper<NationalPupil, IEnumerable<PhonicsOutputRecord>>> phonics = new();
        Mock<IMapper<NationalPupil, IEnumerable<EYFSPOutputRecord>>> eyfsp = new();

        Assert.Throws<ArgumentNullException>(() =>
            new NationalPupilDatabaseAggregationHandler(
                repo.Object,
                autumn.Object,
                summer.Object,
                spring.Object,
                null!,
                ks2.Object,
                ks4.Object,
                mtc.Object,
                phonics.Object,
                eyfsp.Object));
    }

    [Fact]
    public void Constructor_Throws_WhenKs2MapperIsNull()
    {
        Mock<INationalPupilReadOnlyRepository> repo = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusAutumnOutputRecord>>> autumn = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSummerOutputRecord>>> summer = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSpringOutputRecord>>> spring = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS1OutputRecord>>> ks1 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS4OutputRecord>>> ks4 = new();
        Mock<IMapper<NationalPupil, IEnumerable<MTCOutputRecord>>> mtc = new();
        Mock<IMapper<NationalPupil, IEnumerable<PhonicsOutputRecord>>> phonics = new();
        Mock<IMapper<NationalPupil, IEnumerable<EYFSPOutputRecord>>> eyfsp = new();

        Assert.Throws<ArgumentNullException>(() =>
            new NationalPupilDatabaseAggregationHandler(
                repo.Object,
                autumn.Object,
                summer.Object,
                spring.Object,
                ks1.Object,
                null!,
                ks4.Object,
                mtc.Object,
                phonics.Object,
                eyfsp.Object));
    }

    [Fact]
    public void Constructor_Throws_WhenKs4MapperIsNull()
    {
        Mock<INationalPupilReadOnlyRepository> repo = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusAutumnOutputRecord>>> autumn = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSummerOutputRecord>>> summer = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSpringOutputRecord>>> spring = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS1OutputRecord>>> ks1 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS2OutputRecord>>> ks2 = new();
        Mock<IMapper<NationalPupil, IEnumerable<MTCOutputRecord>>> mtc = new();
        Mock<IMapper<NationalPupil, IEnumerable<PhonicsOutputRecord>>> phonics = new();
        Mock<IMapper<NationalPupil, IEnumerable<EYFSPOutputRecord>>> eyfsp = new();

        Assert.Throws<ArgumentNullException>(() =>
            new NationalPupilDatabaseAggregationHandler(
                repo.Object,
                autumn.Object,
                summer.Object,
                spring.Object,
                ks1.Object,
                ks2.Object,
                null!,
                mtc.Object,
                phonics.Object,
                eyfsp.Object));
    }

    [Fact]
    public void Constructor_Throws_WhenMtcMapperIsNull()
    {
        Mock<INationalPupilReadOnlyRepository> repo = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusAutumnOutputRecord>>> autumn = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSummerOutputRecord>>> summer = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSpringOutputRecord>>> spring = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS1OutputRecord>>> ks1 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS2OutputRecord>>> ks2 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS4OutputRecord>>> ks4 = new();
        Mock<IMapper<NationalPupil, IEnumerable<PhonicsOutputRecord>>> phonics = new();
        Mock<IMapper<NationalPupil, IEnumerable<EYFSPOutputRecord>>> eyfsp = new();

        Assert.Throws<ArgumentNullException>(() =>
            new NationalPupilDatabaseAggregationHandler(
                repo.Object,
                autumn.Object,
                summer.Object,
                spring.Object,
                ks1.Object,
                ks2.Object,
                ks4.Object,
                null!,
                phonics.Object,
                eyfsp.Object));
    }

    [Fact]
    public void Constructor_Throws_WhenPhonicsMapperIsNull()
    {
        Mock<INationalPupilReadOnlyRepository> repo = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusAutumnOutputRecord>>> autumn = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSummerOutputRecord>>> summer = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSpringOutputRecord>>> spring = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS1OutputRecord>>> ks1 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS2OutputRecord>>> ks2 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS4OutputRecord>>> ks4 = new();
        Mock<IMapper<NationalPupil, IEnumerable<MTCOutputRecord>>> mtc = new();
        Mock<IMapper<NationalPupil, IEnumerable<EYFSPOutputRecord>>> eyfsp = new();

        Assert.Throws<ArgumentNullException>(() =>
            new NationalPupilDatabaseAggregationHandler(
                repo.Object,
                autumn.Object,
                summer.Object,
                spring.Object,
                ks1.Object,
                ks2.Object,
                ks4.Object,
                mtc.Object,
                null!,
                eyfsp.Object));
    }

    [Fact]
    public void Constructor_Throws_WhenEyfspMapperIsNull()
    {
        Mock<INationalPupilReadOnlyRepository> repo = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusAutumnOutputRecord>>> autumn = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSummerOutputRecord>>> summer = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSpringOutputRecord>>> spring = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS1OutputRecord>>> ks1 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS2OutputRecord>>> ks2 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS4OutputRecord>>> ks4 = new();
        Mock<IMapper<NationalPupil, IEnumerable<MTCOutputRecord>>> mtc = new();
        Mock<IMapper<NationalPupil, IEnumerable<PhonicsOutputRecord>>> phonics = new();

        Assert.Throws<ArgumentNullException>(() =>
            new NationalPupilDatabaseAggregationHandler(
                repo.Object,
                autumn.Object,
                summer.Object,
                spring.Object,
                ks1.Object,
                ks2.Object,
                ks4.Object,
                mtc.Object,
                phonics.Object,
                null!));
    }

    [Fact]
    public void SupportedDownloadType_ReturnsNpd()
    {
        Mock<INationalPupilReadOnlyRepository> repo = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusAutumnOutputRecord>>> autumn = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSummerOutputRecord>>> summer = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSpringOutputRecord>>> spring = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS1OutputRecord>>> ks1 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS2OutputRecord>>> ks2 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS4OutputRecord>>> ks4 = new();
        Mock<IMapper<NationalPupil, IEnumerable<MTCOutputRecord>>> mtc = new();
        Mock<IMapper<NationalPupil, IEnumerable<PhonicsOutputRecord>>> phonics = new();
        Mock<IMapper<NationalPupil, IEnumerable<EYFSPOutputRecord>>> eyfsp = new();

        NationalPupilDatabaseAggregationHandler handler =
            new NationalPupilDatabaseAggregationHandler(
                repo.Object,
                autumn.Object,
                summer.Object,
                spring.Object,
                ks1.Object,
                ks2.Object,
                ks4.Object,
                mtc.Object,
                phonics.Object,
                eyfsp.Object);

        Assert.Equal(DownloadType.NPD, handler.SupportedDownloadType);
    }


    [Fact]
    public async Task AggregateAsync_MapsAutumnData_WhenSelectedAndAvailable()
    {
        Mock<INationalPupilReadOnlyRepository> repo = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusAutumnOutputRecord>>> autumn = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSummerOutputRecord>>> summer = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSpringOutputRecord>>> spring = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS1OutputRecord>>> ks1 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS2OutputRecord>>> ks2 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS4OutputRecord>>> ks4 = new();
        Mock<IMapper<NationalPupil, IEnumerable<MTCOutputRecord>>> mtc = new();
        Mock<IMapper<NationalPupil, IEnumerable<PhonicsOutputRecord>>> phonics = new();
        Mock<IMapper<NationalPupil, IEnumerable<EYFSPOutputRecord>>> eyfsp = new();

        NationalPupil pupil = new()
        {
            CensusAutumn = [new CensusAutumnEntry()]
        };

        List<NationalPupil> pupils = new() { pupil };
        repo.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(pupils);

        List<CensusAutumnOutputRecord> mapped = new() { new CensusAutumnOutputRecord() };
        autumn.Setup(m => m.Map(pupil)).Returns(mapped);

        NationalPupilDatabaseAggregationHandler handler =
            new NationalPupilDatabaseAggregationHandler(
                repo.Object, autumn.Object, summer.Object, spring.Object,
                ks1.Object, ks2.Object, ks4.Object, mtc.Object, phonics.Object, eyfsp.Object);

        List<Dataset> datasets = new() { Dataset.Census_Autumn };

        PupilDatasetCollection result = await handler.AggregateAsync(new List<string>(), datasets);

        Assert.Single(result.CensusAutumn);
        autumn.Verify(m => m.Map(pupil), Times.Once);
    }

    [Fact]
    public async Task AggregateAsync_DoesNotMapAutumn_WhenNotSelected()
    {
        Mock<INationalPupilReadOnlyRepository> repo = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusAutumnOutputRecord>>> autumn = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSummerOutputRecord>>> summer = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSpringOutputRecord>>> spring = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS1OutputRecord>>> ks1 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS2OutputRecord>>> ks2 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS4OutputRecord>>> ks4 = new();
        Mock<IMapper<NationalPupil, IEnumerable<MTCOutputRecord>>> mtc = new();
        Mock<IMapper<NationalPupil, IEnumerable<PhonicsOutputRecord>>> phonics = new();
        Mock<IMapper<NationalPupil, IEnumerable<EYFSPOutputRecord>>> eyfsp = new();

        NationalPupil pupil = new()
        {
            CensusAutumn = [new CensusAutumnEntry()]
        };

        List<NationalPupil> pupils = new() { pupil };
        repo.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(pupils);

        NationalPupilDatabaseAggregationHandler handler =
            new NationalPupilDatabaseAggregationHandler(
                repo.Object, autumn.Object, summer.Object, spring.Object,
                ks1.Object, ks2.Object, ks4.Object, mtc.Object, phonics.Object, eyfsp.Object);

        List<Dataset> datasets = new(); // Not selected

        PupilDatasetCollection result = await handler.AggregateAsync(new List<string>(), datasets);

        Assert.Empty(result.CensusAutumn);
        autumn.Verify(m => m.Map(It.IsAny<NationalPupil>()), Times.Never);
    }

    [Fact]
    public async Task AggregateAsync_DoesNotMapAutumn_WhenPupilHasNoData()
    {
        Mock<INationalPupilReadOnlyRepository> repo = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusAutumnOutputRecord>>> autumn = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSummerOutputRecord>>> summer = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSpringOutputRecord>>> spring = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS1OutputRecord>>> ks1 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS2OutputRecord>>> ks2 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS4OutputRecord>>> ks4 = new();
        Mock<IMapper<NationalPupil, IEnumerable<MTCOutputRecord>>> mtc = new();
        Mock<IMapper<NationalPupil, IEnumerable<PhonicsOutputRecord>>> phonics = new();
        Mock<IMapper<NationalPupil, IEnumerable<EYFSPOutputRecord>>> eyfsp = new();

        NationalPupil pupil = new()
        {
            CensusAutumn = [] // empty list = no data
        };

        List<NationalPupil> pupils = new() { pupil };
        repo.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(pupils);

        NationalPupilDatabaseAggregationHandler handler =
            new NationalPupilDatabaseAggregationHandler(
                repo.Object, autumn.Object, summer.Object, spring.Object,
                ks1.Object, ks2.Object, ks4.Object, mtc.Object, phonics.Object, eyfsp.Object);

        List<Dataset> datasets = new() { Dataset.Census_Autumn };

        PupilDatasetCollection result = await handler.AggregateAsync(new List<string>(), datasets);

        Assert.Empty(result.CensusAutumn);
        autumn.Verify(m => m.Map(It.IsAny<NationalPupil>()), Times.Never);
    }


    [Fact]
    public async Task AggregateAsync_MapsSummerData_WhenSelectedAndAvailable()
    {
        Mock<INationalPupilReadOnlyRepository> repo = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSummerOutputRecord>>> summer = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusAutumnOutputRecord>>> autumn = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSpringOutputRecord>>> spring = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS1OutputRecord>>> ks1 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS2OutputRecord>>> ks2 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS4OutputRecord>>> ks4 = new();
        Mock<IMapper<NationalPupil, IEnumerable<MTCOutputRecord>>> mtc = new();
        Mock<IMapper<NationalPupil, IEnumerable<PhonicsOutputRecord>>> phonics = new();
        Mock<IMapper<NationalPupil, IEnumerable<EYFSPOutputRecord>>> eyfsp = new();

        NationalPupil pupil = new()
        {
            CensusSummer = [new CensusSummerEntry()]
        };

        List<NationalPupil> pupils = new() { pupil };
        repo.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(pupils);

        List<CensusSummerOutputRecord> mapped = new() { new CensusSummerOutputRecord() };
        summer.Setup(m => m.Map(pupil)).Returns(mapped);

        NationalPupilDatabaseAggregationHandler handler =
            new NationalPupilDatabaseAggregationHandler(
                repo.Object, autumn.Object, summer.Object, spring.Object,
                ks1.Object, ks2.Object, ks4.Object, mtc.Object, phonics.Object, eyfsp.Object);

        List<Dataset> datasets = new() { Dataset.Census_Summer };

        PupilDatasetCollection result = await handler.AggregateAsync(new List<string>(), datasets);

        Assert.Single(result.CensusSummer);
        summer.Verify(m => m.Map(pupil), Times.Once);
    }

    [Fact]
    public async Task AggregateAsync_DoesNotMapSummer_WhenNotSelected()
    {
        Mock<INationalPupilReadOnlyRepository> repo = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSummerOutputRecord>>> summer = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusAutumnOutputRecord>>> autumn = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSpringOutputRecord>>> spring = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS1OutputRecord>>> ks1 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS2OutputRecord>>> ks2 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS4OutputRecord>>> ks4 = new();
        Mock<IMapper<NationalPupil, IEnumerable<MTCOutputRecord>>> mtc = new();
        Mock<IMapper<NationalPupil, IEnumerable<PhonicsOutputRecord>>> phonics = new();
        Mock<IMapper<NationalPupil, IEnumerable<EYFSPOutputRecord>>> eyfsp = new();

        NationalPupil pupil = new()
        {
            CensusSummer = [new CensusSummerEntry()]
        };

        List<NationalPupil> pupils = new() { pupil };
        repo.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(pupils);

        NationalPupilDatabaseAggregationHandler handler =
            new NationalPupilDatabaseAggregationHandler(
                repo.Object, autumn.Object, summer.Object, spring.Object,
                ks1.Object, ks2.Object, ks4.Object, mtc.Object, phonics.Object, eyfsp.Object);

        List<Dataset> datasets = new(); // Not selected

        PupilDatasetCollection result = await handler.AggregateAsync(new List<string>(), datasets);

        Assert.Empty(result.CensusSummer);
        summer.Verify(m => m.Map(It.IsAny<NationalPupil>()), Times.Never);
    }

    [Fact]
    public async Task AggregateAsync_DoesNotMapSummer_WhenPupilHasNoData()
    {
        Mock<INationalPupilReadOnlyRepository> repo = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSummerOutputRecord>>> summer = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusAutumnOutputRecord>>> autumn = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSpringOutputRecord>>> spring = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS1OutputRecord>>> ks1 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS2OutputRecord>>> ks2 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS4OutputRecord>>> ks4 = new();
        Mock<IMapper<NationalPupil, IEnumerable<MTCOutputRecord>>> mtc = new();
        Mock<IMapper<NationalPupil, IEnumerable<PhonicsOutputRecord>>> phonics = new();
        Mock<IMapper<NationalPupil, IEnumerable<EYFSPOutputRecord>>> eyfsp = new();

        NationalPupil pupil = new()
        {
            CensusSummer = [] // empty list
        };

        List<NationalPupil> pupils = new() { pupil };
        repo.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(pupils);

        NationalPupilDatabaseAggregationHandler handler =
            new NationalPupilDatabaseAggregationHandler(
                repo.Object, autumn.Object, summer.Object, spring.Object,
                ks1.Object, ks2.Object, ks4.Object, mtc.Object, phonics.Object, eyfsp.Object);

        List<Dataset> datasets = new() { Dataset.Census_Summer };

        PupilDatasetCollection result = await handler.AggregateAsync(new List<string>(), datasets);

        Assert.Empty(result.CensusSummer);
        summer.Verify(m => m.Map(It.IsAny<NationalPupil>()), Times.Never);
    }


    [Fact]
    public async Task AggregateAsync_MapsSpringData_WhenSelectedAndAvailable()
    {
        Mock<INationalPupilReadOnlyRepository> repo = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSpringOutputRecord>>> spring = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusAutumnOutputRecord>>> autumn = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSummerOutputRecord>>> summer = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS1OutputRecord>>> ks1 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS2OutputRecord>>> ks2 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS4OutputRecord>>> ks4 = new();
        Mock<IMapper<NationalPupil, IEnumerable<MTCOutputRecord>>> mtc = new();
        Mock<IMapper<NationalPupil, IEnumerable<PhonicsOutputRecord>>> phonics = new();
        Mock<IMapper<NationalPupil, IEnumerable<EYFSPOutputRecord>>> eyfsp = new();

        NationalPupil pupil = new()
        {
            CensusSpring = [new CensusSpringEntry()]
        };

        List<NationalPupil> pupils = new() { pupil };
        repo.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(pupils);

        List<CensusSpringOutputRecord> mapped = new() { new CensusSpringOutputRecord() };
        spring.Setup(m => m.Map(pupil)).Returns(mapped);

        NationalPupilDatabaseAggregationHandler handler =
            new NationalPupilDatabaseAggregationHandler(
                repo.Object, autumn.Object, summer.Object, spring.Object,
                ks1.Object, ks2.Object, ks4.Object, mtc.Object, phonics.Object, eyfsp.Object);

        List<Dataset> datasets = new() { Dataset.Census_Spring };

        PupilDatasetCollection result = await handler.AggregateAsync(new List<string>(), datasets);

        Assert.Single(result.CensusSpring);
        spring.Verify(m => m.Map(pupil), Times.Once);
    }

    [Fact]
    public async Task AggregateAsync_DoesNotMapSpring_WhenNotSelected()
    {
        Mock<INationalPupilReadOnlyRepository> repo = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSpringOutputRecord>>> spring = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusAutumnOutputRecord>>> autumn = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSummerOutputRecord>>> summer = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS1OutputRecord>>> ks1 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS2OutputRecord>>> ks2 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS4OutputRecord>>> ks4 = new();
        Mock<IMapper<NationalPupil, IEnumerable<MTCOutputRecord>>> mtc = new();
        Mock<IMapper<NationalPupil, IEnumerable<PhonicsOutputRecord>>> phonics = new();
        Mock<IMapper<NationalPupil, IEnumerable<EYFSPOutputRecord>>> eyfsp = new();

        NationalPupil pupil = new()
        {
            CensusSpring = [new CensusSpringEntry()]
        };

        List<NationalPupil> pupils = new() { pupil };
        repo.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(pupils);

        NationalPupilDatabaseAggregationHandler handler =
            new NationalPupilDatabaseAggregationHandler(
                repo.Object, autumn.Object, summer.Object, spring.Object,
                ks1.Object, ks2.Object, ks4.Object, mtc.Object, phonics.Object, eyfsp.Object);

        List<Dataset> datasets = new(); // Not selected

        PupilDatasetCollection result = await handler.AggregateAsync(new List<string>(), datasets);

        Assert.Empty(result.CensusSpring);
        spring.Verify(m => m.Map(It.IsAny<NationalPupil>()), Times.Never);
    }

    [Fact]
    public async Task AggregateAsync_DoesNotMapSpring_WhenPupilHasNoData()
    {
        Mock<INationalPupilReadOnlyRepository> repo = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSpringOutputRecord>>> spring = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusAutumnOutputRecord>>> autumn = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSummerOutputRecord>>> summer = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS1OutputRecord>>> ks1 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS2OutputRecord>>> ks2 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS4OutputRecord>>> ks4 = new();
        Mock<IMapper<NationalPupil, IEnumerable<MTCOutputRecord>>> mtc = new();
        Mock<IMapper<NationalPupil, IEnumerable<PhonicsOutputRecord>>> phonics = new();
        Mock<IMapper<NationalPupil, IEnumerable<EYFSPOutputRecord>>> eyfsp = new();

        NationalPupil pupil = new()
        {
            CensusSpring = [] // empty list
        };

        List<NationalPupil> pupils = new() { pupil };
        repo.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(pupils);

        NationalPupilDatabaseAggregationHandler handler =
            new NationalPupilDatabaseAggregationHandler(
                repo.Object, autumn.Object, summer.Object, spring.Object,
                ks1.Object, ks2.Object, ks4.Object, mtc.Object, phonics.Object, eyfsp.Object);

        List<Dataset> datasets = new() { Dataset.Census_Spring };

        PupilDatasetCollection result = await handler.AggregateAsync(new List<string>(), datasets);

        Assert.Empty(result.CensusSpring);
        spring.Verify(m => m.Map(It.IsAny<NationalPupil>()), Times.Never);
    }


    [Fact]
    public async Task AggregateAsync_MapsKS1Data_WhenSelectedAndAvailable()
    {
        Mock<INationalPupilReadOnlyRepository> repo = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS1OutputRecord>>> ks1 = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusAutumnOutputRecord>>> autumn = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSummerOutputRecord>>> summer = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSpringOutputRecord>>> spring = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS2OutputRecord>>> ks2 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS4OutputRecord>>> ks4 = new();
        Mock<IMapper<NationalPupil, IEnumerable<MTCOutputRecord>>> mtc = new();
        Mock<IMapper<NationalPupil, IEnumerable<PhonicsOutputRecord>>> phonics = new();
        Mock<IMapper<NationalPupil, IEnumerable<EYFSPOutputRecord>>> eyfsp = new();

        NationalPupil pupil = new()
        {
            KeyStage1 = [new KeyStage1Entry()]
        };

        List<NationalPupil> pupils = new() { pupil };
        repo.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(pupils);

        List<KS1OutputRecord> mapped = new() { new KS1OutputRecord() };
        ks1.Setup(m => m.Map(pupil)).Returns(mapped);

        NationalPupilDatabaseAggregationHandler handler =
            new NationalPupilDatabaseAggregationHandler(
                repo.Object, autumn.Object, summer.Object, spring.Object,
                ks1.Object, ks2.Object, ks4.Object, mtc.Object, phonics.Object, eyfsp.Object);

        List<Dataset> datasets = new() { Dataset.KS1 };

        PupilDatasetCollection result = await handler.AggregateAsync(new List<string>(), datasets);

        Assert.Single(result.KS1);
        ks1.Verify(m => m.Map(pupil), Times.Once);
    }

    [Fact]
    public async Task AggregateAsync_DoesNotMapKS1_WhenNotSelected()
    {
        Mock<INationalPupilReadOnlyRepository> repo = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS1OutputRecord>>> ks1 = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusAutumnOutputRecord>>> autumn = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSummerOutputRecord>>> summer = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSpringOutputRecord>>> spring = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS2OutputRecord>>> ks2 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS4OutputRecord>>> ks4 = new();
        Mock<IMapper<NationalPupil, IEnumerable<MTCOutputRecord>>> mtc = new();
        Mock<IMapper<NationalPupil, IEnumerable<PhonicsOutputRecord>>> phonics = new();
        Mock<IMapper<NationalPupil, IEnumerable<EYFSPOutputRecord>>> eyfsp = new();

        NationalPupil pupil = new()
        {
            KeyStage1 = [new KeyStage1Entry()]
        };

        List<NationalPupil> pupils = new() { pupil };
        repo.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(pupils);

        NationalPupilDatabaseAggregationHandler handler =
            new NationalPupilDatabaseAggregationHandler(
                repo.Object, autumn.Object, summer.Object, spring.Object,
                ks1.Object, ks2.Object, ks4.Object, mtc.Object, phonics.Object, eyfsp.Object);

        List<Dataset> datasets = new(); // Not selected

        PupilDatasetCollection result = await handler.AggregateAsync(new List<string>(), datasets);

        Assert.Empty(result.KS1);
        ks1.Verify(m => m.Map(It.IsAny<NationalPupil>()), Times.Never);
    }

    [Fact]
    public async Task AggregateAsync_DoesNotMapKS1_WhenPupilHasNoData()
    {
        Mock<INationalPupilReadOnlyRepository> repo = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS1OutputRecord>>> ks1 = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusAutumnOutputRecord>>> autumn = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSummerOutputRecord>>> summer = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSpringOutputRecord>>> spring = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS2OutputRecord>>> ks2 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS4OutputRecord>>> ks4 = new();
        Mock<IMapper<NationalPupil, IEnumerable<MTCOutputRecord>>> mtc = new();
        Mock<IMapper<NationalPupil, IEnumerable<PhonicsOutputRecord>>> phonics = new();
        Mock<IMapper<NationalPupil, IEnumerable<EYFSPOutputRecord>>> eyfsp = new();

        NationalPupil pupil = new()
        {
            KeyStage1 = [] // empty list
        };

        List<NationalPupil> pupils = new() { pupil };
        repo.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(pupils);

        NationalPupilDatabaseAggregationHandler handler =
            new NationalPupilDatabaseAggregationHandler(
                repo.Object, autumn.Object, summer.Object, spring.Object,
                ks1.Object, ks2.Object, ks4.Object, mtc.Object, phonics.Object, eyfsp.Object);

        List<Dataset> datasets = new() { Dataset.KS1 };

        PupilDatasetCollection result = await handler.AggregateAsync(new List<string>(), datasets);

        Assert.Empty(result.KS1);
        ks1.Verify(m => m.Map(It.IsAny<NationalPupil>()), Times.Never);
    }


    [Fact]
    public async Task AggregateAsync_MapsKS2Data_WhenSelectedAndAvailable()
    {
        Mock<INationalPupilReadOnlyRepository> repo = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS2OutputRecord>>> ks2 = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusAutumnOutputRecord>>> autumn = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSummerOutputRecord>>> summer = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSpringOutputRecord>>> spring = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS1OutputRecord>>> ks1 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS4OutputRecord>>> ks4 = new();
        Mock<IMapper<NationalPupil, IEnumerable<MTCOutputRecord>>> mtc = new();
        Mock<IMapper<NationalPupil, IEnumerable<PhonicsOutputRecord>>> phonics = new();
        Mock<IMapper<NationalPupil, IEnumerable<EYFSPOutputRecord>>> eyfsp = new();

        NationalPupil pupil = new()
        {
            KeyStage2 = [new KeyStage2Entry()]
        };

        List<NationalPupil> pupils = new() { pupil };
        repo.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(pupils);

        List<KS2OutputRecord> mapped = new() { new KS2OutputRecord() };
        ks2.Setup(m => m.Map(pupil)).Returns(mapped);

        NationalPupilDatabaseAggregationHandler handler =
            new NationalPupilDatabaseAggregationHandler(
                repo.Object, autumn.Object, summer.Object, spring.Object,
                ks1.Object, ks2.Object, ks4.Object, mtc.Object, phonics.Object, eyfsp.Object);

        List<Dataset> datasets = new() { Dataset.KS2 };

        PupilDatasetCollection result = await handler.AggregateAsync(new List<string>(), datasets);

        Assert.Single(result.KS2);
        ks2.Verify(m => m.Map(pupil), Times.Once);
    }

    [Fact]
    public async Task AggregateAsync_DoesNotMapKS2_WhenNotSelected()
    {
        Mock<INationalPupilReadOnlyRepository> repo = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS2OutputRecord>>> ks2 = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusAutumnOutputRecord>>> autumn = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSummerOutputRecord>>> summer = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSpringOutputRecord>>> spring = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS1OutputRecord>>> ks1 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS4OutputRecord>>> ks4 = new();
        Mock<IMapper<NationalPupil, IEnumerable<MTCOutputRecord>>> mtc = new();
        Mock<IMapper<NationalPupil, IEnumerable<PhonicsOutputRecord>>> phonics = new();
        Mock<IMapper<NationalPupil, IEnumerable<EYFSPOutputRecord>>> eyfsp = new();

        NationalPupil pupil = new()
        {
            KeyStage2 = [new KeyStage2Entry()]
        };

        List<NationalPupil> pupils = new() { pupil };
        repo.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(pupils);

        NationalPupilDatabaseAggregationHandler handler =
            new NationalPupilDatabaseAggregationHandler(
                repo.Object, autumn.Object, summer.Object, spring.Object,
                ks1.Object, ks2.Object, ks4.Object, mtc.Object, phonics.Object, eyfsp.Object);

        List<Dataset> datasets = new(); // Not selected

        PupilDatasetCollection result = await handler.AggregateAsync(new List<string>(), datasets);

        Assert.Empty(result.KS2);
        ks2.Verify(m => m.Map(It.IsAny<NationalPupil>()), Times.Never);
    }

    [Fact]
    public async Task AggregateAsync_DoesNotMapKS2_WhenPupilHasNoData()
    {
        Mock<INationalPupilReadOnlyRepository> repo = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS2OutputRecord>>> ks2 = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusAutumnOutputRecord>>> autumn = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSummerOutputRecord>>> summer = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSpringOutputRecord>>> spring = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS1OutputRecord>>> ks1 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS4OutputRecord>>> ks4 = new();
        Mock<IMapper<NationalPupil, IEnumerable<MTCOutputRecord>>> mtc = new();
        Mock<IMapper<NationalPupil, IEnumerable<PhonicsOutputRecord>>> phonics = new();
        Mock<IMapper<NationalPupil, IEnumerable<EYFSPOutputRecord>>> eyfsp = new();

        NationalPupil pupil = new()
        {
            KeyStage2 = [] // empty list
        };

        List<NationalPupil> pupils = new() { pupil };
        repo.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(pupils);

        NationalPupilDatabaseAggregationHandler handler =
            new NationalPupilDatabaseAggregationHandler(
                repo.Object, autumn.Object, summer.Object, spring.Object,
                ks1.Object, ks2.Object, ks4.Object, mtc.Object, phonics.Object, eyfsp.Object);

        List<Dataset> datasets = new() { Dataset.KS2 };

        PupilDatasetCollection result = await handler.AggregateAsync(new List<string>(), datasets);

        Assert.Empty(result.KS2);
        ks2.Verify(m => m.Map(It.IsAny<NationalPupil>()), Times.Never);
    }

    [Fact]
    public async Task AggregateAsync_MapsKS4Data_WhenSelectedAndAvailable()
    {
        Mock<INationalPupilReadOnlyRepository> repo = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS4OutputRecord>>> ks4 = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusAutumnOutputRecord>>> autumn = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSummerOutputRecord>>> summer = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSpringOutputRecord>>> spring = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS1OutputRecord>>> ks1 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS2OutputRecord>>> ks2 = new();
        Mock<IMapper<NationalPupil, IEnumerable<MTCOutputRecord>>> mtc = new();
        Mock<IMapper<NationalPupil, IEnumerable<PhonicsOutputRecord>>> phonics = new();
        Mock<IMapper<NationalPupil, IEnumerable<EYFSPOutputRecord>>> eyfsp = new();

        NationalPupil pupil = new()
        {
            KeyStage4 = [new KeyStage4Entry()]
        };

        List<NationalPupil> pupils = new() { pupil };
        repo.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(pupils);

        List<KS4OutputRecord> mapped = new() { new KS4OutputRecord() };
        ks4.Setup(m => m.Map(pupil)).Returns(mapped);

        NationalPupilDatabaseAggregationHandler handler =
            new NationalPupilDatabaseAggregationHandler(
                repo.Object, autumn.Object, summer.Object, spring.Object,
                ks1.Object, ks2.Object, ks4.Object, mtc.Object, phonics.Object, eyfsp.Object);

        List<Dataset> datasets = new() { Dataset.KS4 };

        PupilDatasetCollection result = await handler.AggregateAsync(new List<string>(), datasets);

        Assert.Single(result.KS4);
        ks4.Verify(m => m.Map(pupil), Times.Once);
    }

    [Fact]
    public async Task AggregateAsync_DoesNotMapKS4_WhenNotSelected()
    {
        Mock<INationalPupilReadOnlyRepository> repo = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS4OutputRecord>>> ks4 = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusAutumnOutputRecord>>> autumn = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSummerOutputRecord>>> summer = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSpringOutputRecord>>> spring = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS1OutputRecord>>> ks1 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS2OutputRecord>>> ks2 = new();
        Mock<IMapper<NationalPupil, IEnumerable<MTCOutputRecord>>> mtc = new();
        Mock<IMapper<NationalPupil, IEnumerable<PhonicsOutputRecord>>> phonics = new();
        Mock<IMapper<NationalPupil, IEnumerable<EYFSPOutputRecord>>> eyfsp = new();

        NationalPupil pupil = new()
        {
            KeyStage4 = [new KeyStage4Entry()]
        };

        List<NationalPupil> pupils = new() { pupil };
        repo.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(pupils);

        NationalPupilDatabaseAggregationHandler handler =
            new NationalPupilDatabaseAggregationHandler(
                repo.Object, autumn.Object, summer.Object, spring.Object,
                ks1.Object, ks2.Object, ks4.Object, mtc.Object, phonics.Object, eyfsp.Object);

        List<Dataset> datasets = new(); // Not selected

        PupilDatasetCollection result = await handler.AggregateAsync(new List<string>(), datasets);

        Assert.Empty(result.KS4);
        ks4.Verify(m => m.Map(It.IsAny<NationalPupil>()), Times.Never);
    }

    [Fact]
    public async Task AggregateAsync_DoesNotMapKS4_WhenPupilHasNoData()
    {
        Mock<INationalPupilReadOnlyRepository> repo = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS4OutputRecord>>> ks4 = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusAutumnOutputRecord>>> autumn = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSummerOutputRecord>>> summer = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSpringOutputRecord>>> spring = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS1OutputRecord>>> ks1 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS2OutputRecord>>> ks2 = new();
        Mock<IMapper<NationalPupil, IEnumerable<MTCOutputRecord>>> mtc = new();
        Mock<IMapper<NationalPupil, IEnumerable<PhonicsOutputRecord>>> phonics = new();
        Mock<IMapper<NationalPupil, IEnumerable<EYFSPOutputRecord>>> eyfsp = new();

        NationalPupil pupil = new()
        {
            KeyStage4 = [] // empty list
        };

        List<NationalPupil> pupils = new() { pupil };
        repo.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(pupils);

        NationalPupilDatabaseAggregationHandler handler =
            new NationalPupilDatabaseAggregationHandler(
                repo.Object, autumn.Object, summer.Object, spring.Object,
                ks1.Object, ks2.Object, ks4.Object, mtc.Object, phonics.Object, eyfsp.Object);

        List<Dataset> datasets = new() { Dataset.KS4 };

        PupilDatasetCollection result = await handler.AggregateAsync(new List<string>(), datasets);

        Assert.Empty(result.KS4);
        ks4.Verify(m => m.Map(It.IsAny<NationalPupil>()), Times.Never);
    }


    [Fact]
    public async Task AggregateAsync_MapsMTCData_WhenSelectedAndAvailable()
    {
        Mock<INationalPupilReadOnlyRepository> repo = new();
        Mock<IMapper<NationalPupil, IEnumerable<MTCOutputRecord>>> mtc = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusAutumnOutputRecord>>> autumn = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSummerOutputRecord>>> summer = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSpringOutputRecord>>> spring = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS1OutputRecord>>> ks1 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS2OutputRecord>>> ks2 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS4OutputRecord>>> ks4 = new();
        Mock<IMapper<NationalPupil, IEnumerable<PhonicsOutputRecord>>> phonics = new();
        Mock<IMapper<NationalPupil, IEnumerable<EYFSPOutputRecord>>> eyfsp = new();

        NationalPupil pupil = new()
        {
            MTC = [new MtcEntry()]
        };

        List<NationalPupil> pupils = new() { pupil };
        repo.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(pupils);

        List<MTCOutputRecord> mapped = new() { new MTCOutputRecord() };
        mtc.Setup(m => m.Map(pupil)).Returns(mapped);

        NationalPupilDatabaseAggregationHandler handler =
            new NationalPupilDatabaseAggregationHandler(
                repo.Object, autumn.Object, summer.Object, spring.Object,
                ks1.Object, ks2.Object, ks4.Object, mtc.Object, phonics.Object, eyfsp.Object);

        List<Dataset> datasets = new() { Dataset.MTC };

        PupilDatasetCollection result = await handler.AggregateAsync(new List<string>(), datasets);

        Assert.Single(result.MTC);
        mtc.Verify(m => m.Map(pupil), Times.Once);
    }

    [Fact]
    public async Task AggregateAsync_DoesNotMapMTC_WhenNotSelected()
    {
        Mock<INationalPupilReadOnlyRepository> repo = new();
        Mock<IMapper<NationalPupil, IEnumerable<MTCOutputRecord>>> mtc = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusAutumnOutputRecord>>> autumn = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSummerOutputRecord>>> summer = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSpringOutputRecord>>> spring = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS1OutputRecord>>> ks1 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS2OutputRecord>>> ks2 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS4OutputRecord>>> ks4 = new();
        Mock<IMapper<NationalPupil, IEnumerable<PhonicsOutputRecord>>> phonics = new();
        Mock<IMapper<NationalPupil, IEnumerable<EYFSPOutputRecord>>> eyfsp = new();

        NationalPupil pupil = new()
        {
            MTC = [new MtcEntry()]
        };

        List<NationalPupil> pupils = new() { pupil };
        repo.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(pupils);

        NationalPupilDatabaseAggregationHandler handler =
            new NationalPupilDatabaseAggregationHandler(
                repo.Object, autumn.Object, summer.Object, spring.Object,
                ks1.Object, ks2.Object, ks4.Object, mtc.Object, phonics.Object, eyfsp.Object);

        List<Dataset> datasets = new(); // Not selected

        PupilDatasetCollection result = await handler.AggregateAsync(new List<string>(), datasets);

        Assert.Empty(result.MTC);
        mtc.Verify(m => m.Map(It.IsAny<NationalPupil>()), Times.Never);
    }

    [Fact]
    public async Task AggregateAsync_DoesNotMapMTC_WhenPupilHasNoData()
    {
        Mock<INationalPupilReadOnlyRepository> repo = new();
        Mock<IMapper<NationalPupil, IEnumerable<MTCOutputRecord>>> mtc = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusAutumnOutputRecord>>> autumn = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSummerOutputRecord>>> summer = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSpringOutputRecord>>> spring = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS1OutputRecord>>> ks1 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS2OutputRecord>>> ks2 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS4OutputRecord>>> ks4 = new();
        Mock<IMapper<NationalPupil, IEnumerable<PhonicsOutputRecord>>> phonics = new();
        Mock<IMapper<NationalPupil, IEnumerable<EYFSPOutputRecord>>> eyfsp = new();

        NationalPupil pupil = new()
        {
            MTC = [] // empty list
        };

        List<NationalPupil> pupils = new() { pupil };
        repo.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(pupils);

        NationalPupilDatabaseAggregationHandler handler =
            new NationalPupilDatabaseAggregationHandler(
                repo.Object, autumn.Object, summer.Object, spring.Object,
                ks1.Object, ks2.Object, ks4.Object, mtc.Object, phonics.Object, eyfsp.Object);

        List<Dataset> datasets = new() { Dataset.MTC };

        PupilDatasetCollection result = await handler.AggregateAsync(new List<string>(), datasets);

        Assert.Empty(result.MTC);
        mtc.Verify(m => m.Map(It.IsAny<NationalPupil>()), Times.Never);
    }

    [Fact]
    public async Task AggregateAsync_MapsPhonicsData_WhenSelectedAndAvailable()
    {
        Mock<INationalPupilReadOnlyRepository> repo = new();
        Mock<IMapper<NationalPupil, IEnumerable<PhonicsOutputRecord>>> phonics = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusAutumnOutputRecord>>> autumn = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSummerOutputRecord>>> summer = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSpringOutputRecord>>> spring = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS1OutputRecord>>> ks1 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS2OutputRecord>>> ks2 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS4OutputRecord>>> ks4 = new();
        Mock<IMapper<NationalPupil, IEnumerable<MTCOutputRecord>>> mtc = new();
        Mock<IMapper<NationalPupil, IEnumerable<EYFSPOutputRecord>>> eyfsp = new();

        NationalPupil pupil = new()
        {
            Phonics = [new PhonicsEntry()]
        };

        List<NationalPupil> pupils = new() { pupil };
        repo.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(pupils);

        List<PhonicsOutputRecord> mapped = new() { new PhonicsOutputRecord() };
        phonics.Setup(m => m.Map(pupil)).Returns(mapped);

        NationalPupilDatabaseAggregationHandler handler =
            new NationalPupilDatabaseAggregationHandler(
                repo.Object, autumn.Object, summer.Object, spring.Object,
                ks1.Object, ks2.Object, ks4.Object, mtc.Object, phonics.Object, eyfsp.Object);

        List<Dataset> datasets = new() { Dataset.Phonics };

        PupilDatasetCollection result = await handler.AggregateAsync(new List<string>(), datasets);

        Assert.Single(result.Phonics);
        phonics.Verify(m => m.Map(pupil), Times.Once);
    }

    [Fact]
    public async Task AggregateAsync_DoesNotMapPhonics_WhenNotSelected()
    {
        Mock<INationalPupilReadOnlyRepository> repo = new();
        Mock<IMapper<NationalPupil, IEnumerable<PhonicsOutputRecord>>> phonics = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusAutumnOutputRecord>>> autumn = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSummerOutputRecord>>> summer = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSpringOutputRecord>>> spring = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS1OutputRecord>>> ks1 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS2OutputRecord>>> ks2 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS4OutputRecord>>> ks4 = new();
        Mock<IMapper<NationalPupil, IEnumerable<MTCOutputRecord>>> mtc = new();
        Mock<IMapper<NationalPupil, IEnumerable<EYFSPOutputRecord>>> eyfsp = new();

        NationalPupil pupil = new()
        {
            Phonics = [new PhonicsEntry()]
        };

        List<NationalPupil> pupils = new() { pupil };
        repo.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(pupils);

        NationalPupilDatabaseAggregationHandler handler =
            new NationalPupilDatabaseAggregationHandler(
                repo.Object, autumn.Object, summer.Object, spring.Object,
                ks1.Object, ks2.Object, ks4.Object, mtc.Object, phonics.Object, eyfsp.Object);

        List<Dataset> datasets = new(); // Not selected

        PupilDatasetCollection result = await handler.AggregateAsync(new List<string>(), datasets);

        Assert.Empty(result.Phonics);
        phonics.Verify(m => m.Map(It.IsAny<NationalPupil>()), Times.Never);
    }

    [Fact]
    public async Task AggregateAsync_DoesNotMapPhonics_WhenPupilHasNoData()
    {
        Mock<INationalPupilReadOnlyRepository> repo = new();
        Mock<IMapper<NationalPupil, IEnumerable<PhonicsOutputRecord>>> phonics = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusAutumnOutputRecord>>> autumn = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSummerOutputRecord>>> summer = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSpringOutputRecord>>> spring = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS1OutputRecord>>> ks1 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS2OutputRecord>>> ks2 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS4OutputRecord>>> ks4 = new();
        Mock<IMapper<NationalPupil, IEnumerable<MTCOutputRecord>>> mtc = new();
        Mock<IMapper<NationalPupil, IEnumerable<EYFSPOutputRecord>>> eyfsp = new();

        NationalPupil pupil = new()
        {
            Phonics = [] // empty list
        };

        List<NationalPupil> pupils = new() { pupil };
        repo.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(pupils);

        NationalPupilDatabaseAggregationHandler handler =
            new NationalPupilDatabaseAggregationHandler(
                repo.Object, autumn.Object, summer.Object, spring.Object,
                ks1.Object, ks2.Object, ks4.Object, mtc.Object, phonics.Object, eyfsp.Object);

        List<Dataset> datasets = new() { Dataset.Phonics };

        PupilDatasetCollection result = await handler.AggregateAsync(new List<string>(), datasets);

        Assert.Empty(result.Phonics);
        phonics.Verify(m => m.Map(It.IsAny<NationalPupil>()), Times.Never);
    }

    [Fact]
    public async Task AggregateAsync_MapsEYFSPData_WhenSelectedAndAvailable()
    {
        Mock<INationalPupilReadOnlyRepository> repo = new();
        Mock<IMapper<NationalPupil, IEnumerable<EYFSPOutputRecord>>> eyfsp = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusAutumnOutputRecord>>> autumn = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSummerOutputRecord>>> summer = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSpringOutputRecord>>> spring = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS1OutputRecord>>> ks1 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS2OutputRecord>>> ks2 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS4OutputRecord>>> ks4 = new();
        Mock<IMapper<NationalPupil, IEnumerable<MTCOutputRecord>>> mtc = new();
        Mock<IMapper<NationalPupil, IEnumerable<PhonicsOutputRecord>>> phonics = new();

        NationalPupil pupil = new()
        {
            EarlyYearsFoundationStageProfile =
        [
            new EarlyYearsFoundationStageProfileEntry()
        ]
        };

        List<NationalPupil> pupils = new() { pupil };
        repo.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(pupils);

        List<EYFSPOutputRecord> mapped = new() { new EYFSPOutputRecord() };
        eyfsp.Setup(m => m.Map(pupil)).Returns(mapped);

        NationalPupilDatabaseAggregationHandler handler =
            new NationalPupilDatabaseAggregationHandler(
                repo.Object, autumn.Object, summer.Object, spring.Object,
                ks1.Object, ks2.Object, ks4.Object, mtc.Object, phonics.Object, eyfsp.Object);

        List<Dataset> datasets = new() { Dataset.EYFSP };

        PupilDatasetCollection result = await handler.AggregateAsync(new List<string>(), datasets);

        Assert.Single(result.EYFSP);
        eyfsp.Verify(m => m.Map(pupil), Times.Once);
    }

    [Fact]
    public async Task AggregateAsync_DoesNotMapEYFSP_WhenNotSelected()
    {
        Mock<INationalPupilReadOnlyRepository> repo = new();
        Mock<IMapper<NationalPupil, IEnumerable<EYFSPOutputRecord>>> eyfsp = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusAutumnOutputRecord>>> autumn = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSummerOutputRecord>>> summer = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSpringOutputRecord>>> spring = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS1OutputRecord>>> ks1 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS2OutputRecord>>> ks2 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS4OutputRecord>>> ks4 = new();
        Mock<IMapper<NationalPupil, IEnumerable<MTCOutputRecord>>> mtc = new();
        Mock<IMapper<NationalPupil, IEnumerable<PhonicsOutputRecord>>> phonics = new();

        NationalPupil pupil = new()
        {
            EarlyYearsFoundationStageProfile =
        [
            new EarlyYearsFoundationStageProfileEntry()
        ]
        };

        List<NationalPupil> pupils = new() { pupil };
        repo.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(pupils);

        NationalPupilDatabaseAggregationHandler handler =
            new NationalPupilDatabaseAggregationHandler(
                repo.Object, autumn.Object, summer.Object, spring.Object,
                ks1.Object, ks2.Object, ks4.Object, mtc.Object, phonics.Object, eyfsp.Object);

        List<Dataset> datasets = new(); // Not selected

        PupilDatasetCollection result = await handler.AggregateAsync(new List<string>(), datasets);

        Assert.Empty(result.EYFSP);
        eyfsp.Verify(m => m.Map(It.IsAny<NationalPupil>()), Times.Never);
    }

    [Fact]
    public async Task AggregateAsync_DoesNotMapEYFSP_WhenPupilHasNoData()
    {
        Mock<INationalPupilReadOnlyRepository> repo = new();
        Mock<IMapper<NationalPupil, IEnumerable<EYFSPOutputRecord>>> eyfsp = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusAutumnOutputRecord>>> autumn = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSummerOutputRecord>>> summer = new();
        Mock<IMapper<NationalPupil, IEnumerable<CensusSpringOutputRecord>>> spring = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS1OutputRecord>>> ks1 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS2OutputRecord>>> ks2 = new();
        Mock<IMapper<NationalPupil, IEnumerable<KS4OutputRecord>>> ks4 = new();
        Mock<IMapper<NationalPupil, IEnumerable<MTCOutputRecord>>> mtc = new();
        Mock<IMapper<NationalPupil, IEnumerable<PhonicsOutputRecord>>> phonics = new();

        NationalPupil pupil = new()
        {
            EarlyYearsFoundationStageProfile = [] // empty list
        };

        List<NationalPupil> pupils = new() { pupil };
        repo.Setup(r => r.GetPupilsByIdsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(pupils);

        NationalPupilDatabaseAggregationHandler handler =
            new NationalPupilDatabaseAggregationHandler(
                repo.Object, autumn.Object, summer.Object, spring.Object,
                ks1.Object, ks2.Object, ks4.Object, mtc.Object, phonics.Object, eyfsp.Object);

        List<Dataset> datasets = new() { Dataset.EYFSP };

        PupilDatasetCollection result = await handler.AggregateAsync(new List<string>(), datasets);

        Assert.Empty(result.EYFSP);
        eyfsp.Verify(m => m.Map(It.IsAny<NationalPupil>()), Times.Never);
    }

}
