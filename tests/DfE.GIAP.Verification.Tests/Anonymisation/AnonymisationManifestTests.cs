using DfE.GIAP.Verification.Anonymisation;

namespace DfE.GIAP.Verification.Tests.Anonymisation;

public class AnonymisationManifestTests
{
    [Fact]
    public void Parse_KeepsTheFilesInOrder()
    {
        AnonymisationManifest manifest = AnonymisationManifest.Parse(
            """
            {
                "files": [
                    { "source": "ks2/raw/NPD_KS2_2025_U.zip", "target": "ks2/NPD_KS2_2025_U.zip" },
                    { "source": "cla/ks2/processed/GIAP_CLA_KS2_2025.zip", "target": "cla-ks2/GIAP_CLA_KS2_2025.zip" }
                ]
            }
            """);

        Assert.Equal(["ks2/NPD_KS2_2025_U.zip", "cla-ks2/GIAP_CLA_KS2_2025.zip"], manifest.Files.Select(f => f.Target));
        Assert.Equal("cla-ks2", manifest.Files[1].Folder.Name);
        Assert.Equal(manifest.Files, AnonymisationManifest.Parse(manifest.ToJson()).Files);
    }

    [Theory]
    [InlineData("""{ "files": [] }""")]
    // Not in the old pipeline's raw/ or processed/ folder
    [InlineData("""{ "files": [{ "source": "ks2/upload/NPD_KS2_2025_U.zip", "target": "ks2/NPD_KS2_2025_U.zip" }] }""")]
    // From another folder
    [InlineData("""{ "files": [{ "source": "ks4/raw/NPD_KS2_2025_U.zip", "target": "ks2/NPD_KS2_2025_U.zip" }] }""")]
    // Not a name the folder accepts
    [InlineData("""{ "files": [{ "source": "ks2/raw/NPD_KS4_2025_U.zip", "target": "ks2/NPD_KS4_2025_U.zip" }] }""")]
    [InlineData("""{ "files": [{ "source": "ks2/raw/a.zip", "target": "ks2/NPD_KS2_2025_U.zip" }, { "source": "ks2/raw/b.zip", "target": "ks2/NPD_KS2_2025_U.zip" }] }""")]
    public void Parse_AnInvalidManifest_Throws(string json) =>
        Assert.ThrowsAny<Exception>(() => AnonymisationManifest.Parse(json));
}
