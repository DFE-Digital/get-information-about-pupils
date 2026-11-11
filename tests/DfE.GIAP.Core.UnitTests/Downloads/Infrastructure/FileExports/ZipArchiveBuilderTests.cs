using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DfE.GIAP.Core.Downloads.Infrastructure.FileExports;

namespace DfE.GIAP.Core.UnitTests.Downloads.Infrastructure.FileExports;

public class ZipArchiveBuilderTests
{
    private readonly ZipArchiveBuilder _builder = new();

    private static string ReadEntry(ZipArchiveEntry entry)
    {
        using Stream stream = entry.Open();
        using StreamReader reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    [Fact]
    public async Task CreateZipAsync_CreatesZipWithSingleFile()
    {
        // Arrange
        Dictionary<string, Func<Stream, Task>> files = new()
        {
            ["file1.txt"] = async s =>
            {
                using StreamWriter writer = new StreamWriter(s, leaveOpen: true);
                await writer.WriteAsync("Hello world");
            }
        };

        // Act
        byte[] zipBytes = await _builder.CreateZipAsync(files);

        // Assert
        using MemoryStream ms = new MemoryStream(zipBytes);
        using ZipArchive zip = new ZipArchive(ms, ZipArchiveMode.Read);

        Assert.Single(zip.Entries);
        ZipArchiveEntry? entry = zip.GetEntry("file1.txt");
        Assert.NotNull(entry);

        Assert.Equal("Hello world", ReadEntry(entry));
    }

    [Fact]
    public async Task CreateZipAsync_CreatesZipWithMultipleFiles()
    {
        // Arrange
        Dictionary<string, Func<Stream, Task>> files = new Dictionary<string, Func<Stream, Task>>
        {
            ["a.txt"] = async s =>
            {
                using StreamWriter writer = new StreamWriter(s, leaveOpen: true);
                await writer.WriteAsync("AAA");
            },
            ["b.txt"] = async s =>
            {
                using StreamWriter writer = new StreamWriter(s, leaveOpen: true);
                await writer.WriteAsync("BBB");
            }
        };

        // Act
        byte[] zipBytes = await _builder.CreateZipAsync(files);

        // Assert
        using MemoryStream ms = new MemoryStream(zipBytes);
        using ZipArchive zip = new ZipArchive(ms, ZipArchiveMode.Read);

        Assert.Equal(2, zip.Entries.Count);

        ZipArchiveEntry? entryA = zip.GetEntry("a.txt");
        ZipArchiveEntry? entryB = zip.GetEntry("b.txt");
        Assert.NotNull(entryA);
        Assert.NotNull(entryB);
        Assert.Equal("AAA", ReadEntry(entryA));
        Assert.Equal("BBB", ReadEntry(entryB));
    }

    [Fact]
    public async Task CreateZipAsync_ProducesEmptyZip_WhenNoFilesProvided()
    {
        // Arrange
        Dictionary<string, Func<Stream, Task>> files = new Dictionary<string, Func<Stream, Task>>();

        // Act
        byte[] zipBytes = await _builder.CreateZipAsync(files);

        // Assert
        using MemoryStream ms = new MemoryStream(zipBytes);
        using ZipArchive zip = new ZipArchive(ms, ZipArchiveMode.Read);

        Assert.Empty(zip.Entries);
    }

    [Fact]
    public async Task CreateZipAsync_InvokesWriterDelegates_UsingMoq()
    {
        // Arrange
        Mock<Func<Stream, Task>> mockWriter = new Mock<Func<Stream, Task>>();

        mockWriter
            .Setup(w => w(It.IsAny<Stream>()))
            .Returns(Task.CompletedTask);

        Dictionary<string, Func<Stream, Task>> files = new Dictionary<string, Func<Stream, Task>>
        {
            ["file.txt"] = mockWriter.Object
        };

        // Act
        await _builder.CreateZipAsync(files);

        // Assert
        mockWriter.Verify(w => w(It.IsAny<Stream>()), Times.Once);
    }

    [Fact]
    public async Task CreateZipAsync_WritesCorrectContent_UsingMoqStreamCapture()
    {
        // Arrange
        Mock<Func<Stream, Task>> mockWriter = new Mock<Func<Stream, Task>>();
        mockWriter
            .Setup(w => w(It.IsAny<Stream>()))
            .Returns<Stream>(async s =>
            {
                using StreamWriter writer = new StreamWriter(s, leaveOpen: true);
                await writer.WriteAsync("Captured");
            });

        Dictionary<string, Func<Stream, Task>> files = new Dictionary<string, Func<Stream, Task>>
        {
            ["captured.txt"] = mockWriter.Object
        };

        // Act
        byte[] zipBytes = await _builder.CreateZipAsync(files);

        // Assert
        using MemoryStream ms = new MemoryStream(zipBytes);
        using ZipArchive zip = new ZipArchive(ms, ZipArchiveMode.Read);

        ZipArchiveEntry? entry = zip.GetEntry("captured.txt");
        Assert.NotNull(entry);

        Assert.Equal("Captured", ReadEntry(entry));
    }
}
