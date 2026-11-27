using System.IO.Compression;
using System.Text;
using DfE.GIAP.Core.Ingest;
using DfE.GIAP.Verification.Anonymisation;

namespace DfE.GIAP.Verification.Tests.Anonymisation;

public sealed class ZipAnonymiserTests : IDisposable
{
    private static readonly AnonymisationClassification _classification = AnonymisationClassification.Parse(
        """
        {
            "identifiers": { "pmr": [{ "source": "[A-Z][0-9]{12}", "target": "[A-Z]{2}[0-9]{11}" }] },
            "folders": {
                "ks2": {
                    "subject": "PupilMatchingRef",
                    "transform": { "PupilMatchingRef": "pmr", "SURNAME": "surname" },
                    "keep": ["LA"]
                }
            }
        }
        """);

    private readonly ValueAnonymiser _values = new(ValueAnonymiserTests.Key, _classification.PmrFormats, ValueAnonymiserTests.Names);

    public void Dispose() => _values.Dispose();

    [Fact]
    public void Anonymise_AnonymisesDataFiles_AndCopiesEverythingElse()
    {
        using MemoryStream source = Zip(
            ("NPD_KS2_2025_U_KtS.txt", "PupilMatchingRef\tSURNAME\tLA\nM891313215015\tGilby\t937\n"),
            ("KS2 guidance.docx", "Gilby"),
            ("__MACOSX/._NPD_KS2_2025_U_KtS.txt", "Gilby"),
            ("folder/", null));
        using MemoryStream destination = new();

        ZipAnonymisationResult result = Anonymiser().Anonymise(source, destination, IngestFolder.Get("ks2"));

        Assert.Equal(1, result.Rows["NPD_KS2_2025_U_KtS.txt"]);

        using ZipArchive archive = new(new MemoryStream(destination.ToArray()), ZipArchiveMode.Read);
        Assert.Equal(["NPD_KS2_2025_U_KtS.txt", "KS2 guidance.docx", "__MACOSX/._NPD_KS2_2025_U_KtS.txt", "folder/"], archive.Entries.Select(e => e.FullName));
        Assert.DoesNotContain("Gilby", Read(archive, "NPD_KS2_2025_U_KtS.txt"), StringComparison.Ordinal);
        Assert.EndsWith("\t937\n", Read(archive, "NPD_KS2_2025_U_KtS.txt"), StringComparison.Ordinal);
        Assert.Equal("Gilby", Read(archive, "KS2 guidance.docx"));
    }

    [Fact]
    public void Anonymise_AFolderThatIsntClassified_Throws()
    {
        using MemoryStream source = Zip(("NPD_KS4_2025_U_KtS.txt", "PupilMatchingRef\n"));

        Assert.Throws<AnonymisationException>(() => Anonymiser().Anonymise(source, new MemoryStream(), IngestFolder.Get("ks4")));
    }

    [Fact]
    public void Anonymise_ADataFileThatCantBeAnonymised_Throws_NamingIt()
    {
        using MemoryStream source = Zip(("NPD_KS2_2025_U_KtS.txt", "PupilMatchingRef\tAddress1\n"));

        AnonymisationException exception = Assert.Throws<AnonymisationException>(() => Anonymiser().Anonymise(source, new MemoryStream(), IngestFolder.Get("ks2")));

        Assert.StartsWith("NPD_KS2_2025_U_KtS.txt: ", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Profile_DescribesTheColumnsAndShapes_WithoutValues()
    {
        using MemoryStream source = Zip(("NPD_KS2_2025_U_KtS.txt", "PupilMatchingRef\tSURNAME\tLA\tAddress1\nM891313215015\tGilby\t937\t1 High St\n"));
        StringWriter output = new();

        ZipProfiler.Profile(source, IngestFolder.Get("ks2"), _classification, output);

        string profile = output.ToString();
        Assert.Contains("`PupilMatchingRef`: Pmr", profile, StringComparison.Ordinal);
        Assert.Contains("shape `A999999999999`: 1", profile, StringComparison.Ordinal);
        Assert.Contains("`Address1`: **not classified**", profile, StringComparison.Ordinal);
        Assert.DoesNotContain("M891313215015", profile, StringComparison.Ordinal);
        Assert.DoesNotContain("Gilby", profile, StringComparison.Ordinal);
        Assert.DoesNotContain("High", profile, StringComparison.Ordinal);
    }

    private ZipAnonymiser Anonymiser() => new(new DataFileAnonymiser(_values), _classification);

    private static MemoryStream Zip(params (string Name, string? Content)[] entries)
    {
        MemoryStream zip = new();
        using (ZipArchive archive = new(zip, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach ((string name, string? content) in entries)
            {
                ZipArchiveEntry entry = archive.CreateEntry(name);
                if (content is not null)
                {
                    using Stream stream = entry.Open();
                    stream.Write(Encoding.UTF8.GetBytes(content));
                }
            }
        }

        zip.Position = 0;
        return zip;
    }

    private static string Read(ZipArchive archive, string name)
    {
        using StreamReader reader = new(archive.GetEntry(name)!.Open());
        return reader.ReadToEnd();
    }
}
