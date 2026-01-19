using System.Text;
using DfE.GIAP.Core.Downloads.Application.Enums;
using DfE.GIAP.Core.Downloads.Infrastructure.FileExports;

namespace DfE.GIAP.Core.UnitTests.Downloads.Infrastructure.FileExports;

public class DelimitedFileExporterTests
{
    private readonly DelimitedFileExporter _exporter = new();

    private static string ReadStream(Stream stream)
    {
        stream.Position = 0;
        using StreamReader reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
        return reader.ReadToEnd();
    }

    private class TestRecord
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
    }

    [Fact]
    public async Task ExportAsync_Writes_CsvHeaderAndRows()
    {
        // Arrange
        List<TestRecord> records = new List<TestRecord>
        {
            new TestRecord { Name = "Alice", Age = 30 },
            new TestRecord { Name = "Bob", Age = 40 }
        };

        using MemoryStream stream = new MemoryStream();

        // Act
        await _exporter.ExportAsync(records, FileFormat.Csv, stream);
        string output = ReadStream(stream);

        // Assert
        string expected =
            "Name,Age" + Environment.NewLine +
            "Alice,30" + Environment.NewLine +
            "Bob,40" + Environment.NewLine;

        Assert.Equal(expected, output);
    }

    [Fact]
    public async Task ExportAsync_Writes_TsvHeaderAndRows()
    {
        // Arrange
        List<TestRecord> records = new List<TestRecord>
        {
            new TestRecord { Name = "Charlie", Age = 25 }
        };

        using MemoryStream stream = new MemoryStream();

        // Act
        await _exporter.ExportAsync(records, FileFormat.Tab, stream);
        string output = ReadStream(stream);

        // Assert
        string expected =
            "Name\tAge" + Environment.NewLine +
            "Charlie\t25" + Environment.NewLine;

        Assert.Equal(expected, output);
    }

    [Fact]
    public async Task ExportAsync_InfersType_WhenTIsObject()
    {
        // Arrange
        List<object> records = new List<object>
        {
            new TestRecord { Name = "Dana", Age = 22 }
        };

        using MemoryStream stream = new MemoryStream();

        // Act
        await _exporter.ExportAsync(records, FileFormat.Csv, stream);
        string output = ReadStream(stream);

        // Assert
        string expected =
            "Name,Age" + Environment.NewLine +
            "Dana,22" + Environment.NewLine;

        Assert.Equal(expected, output);
    }

    [Fact]
    public async Task ExportAsync_Writes_HeaderOnly_WhenNoRecords()
    {
        // Arrange
        IEnumerable<TestRecord> records = Enumerable.Empty<TestRecord>();
        using MemoryStream stream = new MemoryStream();

        // Act
        await _exporter.ExportAsync(records, FileFormat.Csv, stream);
        string output = ReadStream(stream);

        // Assert
        Assert.Equal("Name,Age" + Environment.NewLine, output);
    }

    private class NullRecord
    {
        public string? Value1 { get; set; }
        public string? Value2 { get; set; }
    }

    [Fact]
    public async Task ExportAsync_Handles_NullPropertyValues()
    {
        // Arrange
        List<NullRecord> records = new List<NullRecord>
        {
            new NullRecord { Value1 = null, Value2 = "Test" }
        };

        using MemoryStream stream = new MemoryStream();

        // Act
        await _exporter.ExportAsync(records, FileFormat.Csv, stream);
        string output = ReadStream(stream);

        // Assert
        string expected =
            "Value1,Value2" + Environment.NewLine +
            ",Test" + Environment.NewLine;

        Assert.Equal(expected, output);
    }
}
