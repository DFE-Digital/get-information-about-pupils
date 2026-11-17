using System.Text;
using DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;
using DfE.GIAP.Core.Gias;
using AllEstablishmentDataColumns = DfE.GIAP.Core.Gias.GiasExtractParser.AllEstablishmentDataColumns;
using AllGroupLinksColumns = DfE.GIAP.Core.Gias.GiasExtractParser.AllGroupLinksColumns;

namespace DfE.GIAP.Core.UnitTests.Gias;

public class GiasExtractParserTests
{
    private static readonly Encoding _windows1252 = CodePagesEncodingProvider.Instance.GetEncoding(1252)!;

    [Fact]
    public void ParseAllEstablishmentData_ParsesEveryRow()
    {
        using Stream stream = CreateCsv(
            AllEstablishmentDataColumns.All,
            EstablishmentRow(urn: "100000", name: "Haute Valleé School", laCode: "201", laName: "City of London", closeDate: "31-08-2021"),
            EstablishmentRow(
                urn: "100001",
                laCode: "201",
                laName: "City of London",
                establishmentNumber: "",
                uprn: "",
                statutoryLowAge: "",
                statutoryHighAge: "",
                gorCode: "W"));

        GiasEstablishmentsExtract extract = GiasExtractParser.ParseAllEstablishmentData(stream);

        LocalAuthority localAuthority = Assert.Single(extract.LocalAuthorities);
        Assert.Equal(201, localAuthority.LaCode);
        Assert.Equal("City of London", localAuthority.Name);

        Assert.Collection(
            extract.Establishments.OrderBy(e => e.Urn),
            e =>
            {
                Assert.Equal(100000, e.Urn);
                Assert.Equal(201, e.LaCode);
                Assert.Equal(3614, e.EstablishmentNumber);
                Assert.Equal("Haute Valleé School", e.Name);
                Assert.Equal(200000071925L, e.Uprn);
                Assert.Equal(2, e.TypeOfEstablishmentCode);
                Assert.Equal("Voluntary aided school", e.TypeOfEstablishmentName);
                Assert.Equal(1, e.EstablishmentStatusCode);
                Assert.Equal("Open", e.EstablishmentStatusName);
                Assert.Equal(new DateOnly(2021, 8, 31), e.CloseDate);
                Assert.Equal(2, e.PhaseOfEducationCode);
                Assert.Equal("Primary", e.PhaseOfEducationName);
                Assert.Equal(3, e.StatutoryLowAge);
                Assert.Equal(11, e.StatutoryHighAge);
                Assert.Equal("H", e.GorCode);
            },
            e =>
            {
                Assert.Equal(100001, e.Urn);
                Assert.Null(e.EstablishmentNumber);
                Assert.Null(e.Uprn);
                Assert.Null(e.CloseDate);
                Assert.Null(e.StatutoryLowAge);
                Assert.Null(e.StatutoryHighAge);
                Assert.Equal("W", e.GorCode);
            });
    }

    [Fact]
    public void ParseAllEstablishmentData_IgnoresColumnsItDoesNotUse()
    {
        using Stream stream = CreateCsv([.. AllEstablishmentDataColumns.All, "SchoolCapacity"], [.. EstablishmentRow(), "210"]);

        GiasEstablishmentsExtract extract = GiasExtractParser.ParseAllEstablishmentData(stream);

        Assert.Single(extract.Establishments);
    }

    [Fact]
    public void ParseAllEstablishmentData_ThrowsWhenStreamIsEmpty()
    {
        using MemoryStream stream = new();

        GiasExtractException ex = Assert.Throws<GiasExtractException>(() => GiasExtractParser.ParseAllEstablishmentData(stream));

        Assert.Equal(GiasExtractType.AllEstablishmentData, ex.ExtractType);
    }

    [Fact]
    public void ParseAllEstablishmentData_ThrowsWhenThereAreNoRows()
    {
        using Stream stream = CreateCsv(AllEstablishmentDataColumns.All);

        GiasExtractException ex = Assert.Throws<GiasExtractException>(() => GiasExtractParser.ParseAllEstablishmentData(stream));

        Assert.Contains("no rows", ex.Message);
    }

    [Fact]
    public void ParseAllEstablishmentData_ThrowsWhenAColumnIsMissing()
    {
        string[] columns = [.. AllEstablishmentDataColumns.All.Where(c => c != AllEstablishmentDataColumns.GorCode)];
        using Stream stream = CreateCsv(columns, [.. EstablishmentRow().Take(columns.Length)]);

        GiasExtractException ex = Assert.Throws<GiasExtractException>(() => GiasExtractParser.ParseAllEstablishmentData(stream));

        Assert.Contains("'GOR (code)'", ex.Message);
    }

    [Fact]
    public void ParseAllEstablishmentData_ThrowsWhenAValueIsNotAnInteger()
    {
        using Stream stream = CreateCsv(AllEstablishmentDataColumns.All, EstablishmentRow(), EstablishmentRow(urn: "10000X"));

        GiasExtractException ex = Assert.Throws<GiasExtractException>(() => GiasExtractParser.ParseAllEstablishmentData(stream));

        Assert.Contains("Row 3", ex.Message);
        Assert.Contains("'URN'", ex.Message);
    }

    [Fact]
    public void ParseAllEstablishmentData_ThrowsWhenARequiredValueIsEmpty()
    {
        using Stream stream = CreateCsv(AllEstablishmentDataColumns.All, EstablishmentRow(name: ""));

        GiasExtractException ex = Assert.Throws<GiasExtractException>(() => GiasExtractParser.ParseAllEstablishmentData(stream));

        Assert.Contains("'EstablishmentName' is empty", ex.Message);
    }

    [Fact]
    public void ParseAllEstablishmentData_ThrowsWhenADateIsInvalid()
    {
        using Stream stream = CreateCsv(AllEstablishmentDataColumns.All, EstablishmentRow(closeDate: "2021-08-31"));

        GiasExtractException ex = Assert.Throws<GiasExtractException>(() => GiasExtractParser.ParseAllEstablishmentData(stream));

        Assert.Contains("'CloseDate' is not a valid date", ex.Message);
    }

    [Fact]
    public void ParseAllEstablishmentData_ThrowsWhenAUrnIsDuplicated()
    {
        using Stream stream = CreateCsv(AllEstablishmentDataColumns.All, EstablishmentRow(urn: "100000"), EstablishmentRow(urn: "100000"));

        GiasExtractException ex = Assert.Throws<GiasExtractException>(() => GiasExtractParser.ParseAllEstablishmentData(stream));

        Assert.Contains("URN 100000 appears more than once", ex.Message);
    }

    [Fact]
    public void ParseAllEstablishmentData_ThrowsWhenALocalAuthorityHasMoreThanOneName()
    {
        using Stream stream = CreateCsv(
            AllEstablishmentDataColumns.All,
            EstablishmentRow(urn: "100000", laCode: "201", laName: "City of London"),
            EstablishmentRow(urn: "100001", laCode: "201", laName: "Camden"));

        GiasExtractException ex = Assert.Throws<GiasExtractException>(() => GiasExtractParser.ParseAllEstablishmentData(stream));

        Assert.Contains("LA code 201 has more than one name", ex.Message);
    }

    [Fact]
    public void ParseAllGroupLinks_ParsesEveryRow()
    {
        using Stream stream = CreateCsv(
            AllGroupLinksColumns.All,
            GroupLinkRow(groupUid: "1000", urn: "100000", joinedDate: "01/09/2015"),
            GroupLinkRow(groupUid: "1000", urn: "100001", joinedDate: ""),
            GroupLinkRow(
                groupUid: "2000",
                urn: "100000",
                groupId: "",
                groupName: "Closed Federation",
                groupTypeCode: "01",
                groupTypeName: "Federation",
                groupStatusCode: "CLOSED",
                groupStatusName: "Closed",
                closedDate: "31/08/2020"));

        GiasGroupLinksExtract extract = GiasExtractParser.ParseAllGroupLinks(stream);

        Assert.Collection(
            extract.Groups.OrderBy(g => g.GroupUid),
            g =>
            {
                Assert.Equal(1000, g.GroupUid);
                Assert.Equal("TR00001", g.GroupId);
                Assert.Equal("Example Academy Trust", g.Name);
                Assert.Equal(EstablishmentGroup.MultiAcademyTrustGroupTypeCode, g.GroupTypeCode);
                Assert.Equal("Multi-academy trust", g.GroupTypeName);
                Assert.Equal(EstablishmentGroup.OpenGroupStatusCode, g.GroupStatusCode);
                Assert.Equal("Open", g.GroupStatusName);
                Assert.Null(g.ClosedDate);
            },
            g =>
            {
                Assert.Equal(2000, g.GroupUid);
                Assert.Null(g.GroupId);
                Assert.Equal(1, g.GroupTypeCode);
                Assert.Equal("CLOSED", g.GroupStatusCode);
                Assert.Equal(new DateOnly(2020, 8, 31), g.ClosedDate);
            });

        Assert.Collection(
            extract.Links.OrderBy(l => l.GroupUid).ThenBy(l => l.Urn),
            l =>
            {
                Assert.Equal((1000, 100000), (l.GroupUid, l.Urn));
                Assert.Equal(new DateOnly(2015, 9, 1), l.JoinedDate);
            },
            l =>
            {
                Assert.Equal((1000, 100001), (l.GroupUid, l.Urn));
                Assert.Null(l.JoinedDate);
            },
            l => Assert.Equal((2000, 100000), (l.GroupUid, l.Urn)));
    }

    [Fact]
    public void ParseAllGroupLinks_ThrowsWhenThereAreNoRows()
    {
        using Stream stream = CreateCsv(AllGroupLinksColumns.All);

        GiasExtractException ex = Assert.Throws<GiasExtractException>(() => GiasExtractParser.ParseAllGroupLinks(stream));

        Assert.Equal(GiasExtractType.AllGroupLinks, ex.ExtractType);
        Assert.Contains("no rows", ex.Message);
    }

    [Fact]
    public void ParseAllGroupLinks_ThrowsWhenAColumnIsMissing()
    {
        string[] columns = [.. AllGroupLinksColumns.All.Where(c => c != AllGroupLinksColumns.GroupUid)];
        using Stream stream = CreateCsv(columns);

        GiasExtractException ex = Assert.Throws<GiasExtractException>(() => GiasExtractParser.ParseAllGroupLinks(stream));

        Assert.Contains("'Group UID'", ex.Message);
    }

    [Fact]
    public void ParseAllGroupLinks_ThrowsWhenAGroupHasDifferentDetailsOnDifferentRows()
    {
        using Stream stream = CreateCsv(
            AllGroupLinksColumns.All,
            GroupLinkRow(groupUid: "1000", urn: "100000", groupName: "Example Academy Trust"),
            GroupLinkRow(groupUid: "1000", urn: "100001", groupName: "Renamed Academy Trust"));

        GiasExtractException ex = Assert.Throws<GiasExtractException>(() => GiasExtractParser.ParseAllGroupLinks(stream));

        Assert.Contains("Group UID 1000 has different details", ex.Message);
    }

    [Fact]
    public void ParseAllGroupLinks_ThrowsWhenALinkIsDuplicated()
    {
        using Stream stream = CreateCsv(
            AllGroupLinksColumns.All,
            GroupLinkRow(groupUid: "1000", urn: "100000"),
            GroupLinkRow(groupUid: "1000", urn: "100000"));

        GiasExtractException ex = Assert.Throws<GiasExtractException>(() => GiasExtractParser.ParseAllGroupLinks(stream));

        Assert.Contains("URN 100000 is linked to group UID 1000 more than once", ex.Message);
    }

    internal static string[] EstablishmentRow(
        string urn = "100000",
        string laCode = "201",
        string laName = "City of London",
        string establishmentNumber = "3614",
        string name = "Example School",
        string uprn = "200000071925",
        string closeDate = "",
        string statutoryLowAge = "3",
        string statutoryHighAge = "11",
        string gorCode = "H") =>
        // In the same order as AllEstablishmentDataColumns.All
        [
            urn, laCode, laName, establishmentNumber, name, uprn, "2", "Voluntary aided school", "1", "Open", closeDate, "2", "Primary",
            statutoryLowAge, statutoryHighAge, gorCode
        ];

    internal static string[] GroupLinkRow(
        string groupUid = "1000",
        string urn = "100000",
        string groupId = "TR00001",
        string groupName = "Example Academy Trust",
        string groupTypeCode = "06",
        string groupTypeName = "Multi-academy trust",
        string closedDate = "",
        string groupStatusCode = "OPEN",
        string groupStatusName = "Open",
        string joinedDate = "01/09/2015") =>
        // In the same order as AllGroupLinksColumns.All
        [urn, groupUid, groupId, groupName, groupTypeCode, groupTypeName, closedDate, groupStatusCode, groupStatusName, joinedDate];

    internal static Stream CreateCsv(IEnumerable<string> header, params IEnumerable<string>[] rows)
    {
        StringBuilder csv = new();
        csv.AppendLine(ToCsvLine(header));
        foreach (IEnumerable<string> row in rows)
        {
            csv.AppendLine(ToCsvLine(row));
        }

        return new MemoryStream(_windows1252.GetBytes(csv.ToString()));

        static string ToCsvLine(IEnumerable<string> values) => string.Join(",", values.Select(v => $"\"{v.Replace("\"", "\"\"")}\""));
    }
}
