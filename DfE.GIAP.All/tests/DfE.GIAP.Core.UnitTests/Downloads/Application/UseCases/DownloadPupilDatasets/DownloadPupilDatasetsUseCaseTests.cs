using System.Text;
using DfE.GIAP.Core.Downloads.Application.Aggregators;
using DfE.GIAP.Core.Downloads.Application.Enums;
using DfE.GIAP.Core.Downloads.Application.FileExports;
using DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilDatasets;

namespace DfE.GIAP.Core.UnitTests.Downloads.Application.UseCases.DownloadPupilDatasets;

public sealed class DownloadPupilDataUseCaseTests
{
    [Fact]
    public void Constructor_Throws_WhenAggregatorFactoryIsNull()
    {
        Mock<IDelimitedFileExporter> fileExporter = new();
        Mock<IZipArchiveBuilder> zipBuilder = new();

        Action act = () => new DownloadPupilDataUseCase(
            null!,
            fileExporter.Object,
            zipBuilder.Object);

        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void Constructor_Throws_WhenFileExporterIsNull()
    {
        Mock<IPupilDatasetAggregatorFactory> factory = new();
        Mock<IZipArchiveBuilder> zipBuilder = new();

        Action act = () => new DownloadPupilDataUseCase(
            factory.Object,
            null!,
            zipBuilder.Object);

        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void Constructor_Throws_WhenZipBuilderIsNull()
    {
        Mock<IPupilDatasetAggregatorFactory> factory = new();
        Mock<IDelimitedFileExporter> fileExporter = new();

        Action act = () => new DownloadPupilDataUseCase(
            factory.Object,
            fileExporter.Object,
            null!);

        Assert.Throws<ArgumentNullException>(act);
    }


    [Fact]
    public async Task HandleRequestAsync_ReturnsEmptyResponse_When_NoRecordsExist()
    {
        // Arrange
        Mock<IPupilDatasetAggregatorFactory> factory = new();
        Mock<IDelimitedFileExporter> fileExporter = new();
        Mock<IZipArchiveBuilder> zipBuilder = new();

        DownloadPupilDataRequest request = new(
            SelectedPupils: ["A"],
            SelectedDatasets: [Dataset.FE_PP],
            DownloadType: DownloadType.FurtherEducation,
            FileFormat: FileFormat.Csv);

        factory.Setup(f => f.AggregateAsync(
                request.DownloadType,
                request.SelectedPupils,
                request.SelectedDatasets))
            .ReturnsAsync(new PupilDatasetCollection());

        DownloadPupilDataUseCase sut = new(
            factory.Object,
            fileExporter.Object,
            zipBuilder.Object);

        // Act
        DownloadPupilDataResponse result = await sut.HandleRequestAsync(request);

        // Assert
        Assert.Null(result.FileContents);
        Assert.Null(result.FileName);
        Assert.Null(result.ContentType);
    }


    [Fact]
    public async Task HandleRequestAsync_ReturnsSingleFile_When_OneDatasetHasRecords()
    {
        Mock<IPupilDatasetAggregatorFactory> factory = new();
        Mock<IDelimitedFileExporter> fileExporter = new();
        Mock<IZipArchiveBuilder> zipBuilder = new();

        DownloadPupilDataRequest request = new(
            SelectedPupils: ["A"],
            SelectedDatasets: [Dataset.FE_PP],
            DownloadType: DownloadType.FurtherEducation,
            FileFormat: FileFormat.Csv);

        PupilDatasetCollection datasets = new();
        datasets.FurtherEducationPP.Add(new()
        {
            ULN = "A",
        });

        factory.Setup(f => f.AggregateAsync(
                request.DownloadType,
                request.SelectedPupils,
                request.SelectedDatasets))
            .ReturnsAsync(datasets);

        byte[] exportedBytes = Encoding.UTF8.GetBytes("test");
        fileExporter
            .Setup(e => e.ExportAsync(It.IsAny<IEnumerable<object>>(), FileFormat.Csv, It.IsAny<Stream>()))
            .Callback<IEnumerable<object>, FileFormat, Stream>((records, format, stream) =>
            {
                stream.Write(exportedBytes, 0, exportedBytes.Length);
            })
            .Returns(Task.CompletedTask);

        DownloadPupilDataUseCase sut = new(factory.Object, fileExporter.Object, zipBuilder.Object);

        DownloadPupilDataResponse result = await sut.HandleRequestAsync(request);

        Assert.Equal("pp_results.csv", result.FileName);
        Assert.Equal("text/csv", result.ContentType);
        Assert.Equal(exportedBytes, result.FileContents);
    }

    [Fact]
    public async Task HandleRequestAsync_ReturnsZip_When_MultipleDatasetsHaveRecords()
    {
        // Arrange
        Mock<IPupilDatasetAggregatorFactory> factory = new();
        Mock<IDelimitedFileExporter> fileExporter = new();
        Mock<IZipArchiveBuilder> zipBuilder = new();

        DownloadPupilDataRequest request = new(
            SelectedPupils: ["A"],
            SelectedDatasets: [Dataset.FE_PP, Dataset.SEN],
            DownloadType: DownloadType.FurtherEducation,
            FileFormat: FileFormat.Csv);

        PupilDatasetCollection datasets = new();
        datasets.FurtherEducationPP.Add(new()
        {
            ULN = "A",
        });
        datasets.SEN.Add(new()
        {
            ULN = "A",
        });

        factory.Setup(f => f.AggregateAsync(
                request.DownloadType,
                request.SelectedPupils,
                request.SelectedDatasets))
            .ReturnsAsync(datasets);

        byte[] zipBytes = Encoding.UTF8.GetBytes("zipcontent");
        zipBuilder
            .Setup(z => z.CreateZipAsync(It.IsAny<Dictionary<string, Func<Stream, Task>>>()))
            .ReturnsAsync(zipBytes);

        DownloadPupilDataUseCase sut = new(factory.Object, fileExporter.Object, zipBuilder.Object);

        // Act
        DownloadPupilDataResponse result = await sut.HandleRequestAsync(request);

        // Assert
        Assert.Equal("fe_results.zip", result.FileName);
        Assert.Equal("application/zip", result.ContentType);
        Assert.Equal(zipBytes, result.FileContents);
    }


    [Fact]
    public async Task HandleRequestAsync_CallsAggregatorWithCorrectArguments()
    {
        Mock<IPupilDatasetAggregatorFactory> factory = new();
        Mock<IDelimitedFileExporter> fileExporter = new();
        Mock<IZipArchiveBuilder> zipBuilder = new();

        DownloadPupilDataRequest request = new(
            SelectedPupils: ["A"],
            SelectedDatasets: [Dataset.PP],
            DownloadType: DownloadType.PupilPremium,
            FileFormat: FileFormat.Csv);

        factory.Setup(f => f.AggregateAsync(
                request.DownloadType,
                request.SelectedPupils,
                request.SelectedDatasets))
            .ReturnsAsync(new PupilDatasetCollection());

        DownloadPupilDataUseCase sut = new(factory.Object, fileExporter.Object, zipBuilder.Object);

        await sut.HandleRequestAsync(request);

        factory.Verify(f => f.AggregateAsync(
            request.DownloadType,
            request.SelectedPupils,
            request.SelectedDatasets),
            Times.Once);
    }
}
