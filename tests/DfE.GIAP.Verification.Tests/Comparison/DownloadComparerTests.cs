using System.Text;
using DfE.GIAP.Verification.Comparison;

namespace DfE.GIAP.Verification.Tests.Comparison;

public class DownloadComparerTests
{
    private const string Path = "LA/201/pp2425_201_1.csv";

    [Fact]
    public void Compare_SameFile_HasNoDifferences()
    {
        byte[] content = Encoding.UTF8.GetBytes("UPN,Surname\nA1,Smith\nA2,Jones\n");

        Assert.Empty(DownloadComparer.Compare(Path, content, content));
    }

    [Fact]
    public void Compare_DifferentEncodingAndLayout_ReportsEachDifference()
    {
        byte[] newContent = Encoding.UTF8.GetBytes("UPN,Surname\nA1,Smith\nA2,Jones\n");
        byte[] oldContent = [0xEF, 0xBB, 0xBF, .. Encoding.UTF8.GetBytes("UPN,Surname\r\nA2,Jones\r\nA1,Smith\r\n")];

        Assert.Equal(
            [DifferenceKinds.BomDifferent, DifferenceKinds.LineEndingsDifferent, DifferenceKinds.RowOrder],
            DownloadComparer.Compare(Path, newContent, oldContent).Select(d => d.Kind));
    }

    [Fact]
    public void Compare_DifferentValue_ReportsTheColumnOfThePupilsRow()
    {
        byte[] newContent = Encoding.UTF8.GetBytes("UPN,Surname,NCYear\nA1,Smith,5\nA2,Jones,6\n");
        byte[] oldContent = Encoding.UTF8.GetBytes("UPN,Surname,NCYear\nA1,Smith,5\nA2,Jones,7\n");

        Difference difference = Assert.Single(DownloadComparer.Compare(Path, newContent, oldContent));

        Assert.Equal(
            new Difference(DownloadComparer.Comparison, DifferenceKinds.ValueDifferent, "LA", "NCYear", Path, "\"6\"", "\"7\""),
            difference);
    }

    [Fact]
    public void Compare_DifferentHeader_ReportsIt()
    {
        byte[] newContent = Encoding.UTF8.GetBytes("UPN,Surname\n");
        byte[] oldContent = Encoding.UTF8.GetBytes("Surname,UPN\n");

        Difference difference = Assert.Single(DownloadComparer.Compare(Path, newContent, oldContent));

        Assert.Equal(DifferenceKinds.HeaderDifferent, difference.Kind);
        Assert.Equal("""["UPN","Surname"]""", difference.NewValue);
    }
}
