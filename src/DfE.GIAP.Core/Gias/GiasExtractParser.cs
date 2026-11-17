using System.Globalization;
using System.Text;
using CsvHelper;
using DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;

namespace DfE.GIAP.Core.Gias;

public static class GiasExtractParser
{
    // GIAS extracts are Windows-1252 encoded
    private static readonly Encoding _encoding = CodePagesEncodingProvider.Instance.GetEncoding(1252)!;

    // The all establishment data extract uses dd-MM-yyyy and the all group links extract uses dd/MM/yyyy
    private static readonly string[] _dateFormats = ["dd-MM-yyyy", "dd/MM/yyyy"];

    public static GiasEstablishmentsExtract ParseAllEstablishmentData(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        Dictionary<int, LocalAuthority> localAuthorities = [];
        Dictionary<int, Establishment> establishments = [];

        foreach (GiasCsvRow row in ReadRows(stream, GiasExtractType.AllEstablishmentData, AllEstablishmentDataColumns.All))
        {
            int laCode = row.GetInt(AllEstablishmentDataColumns.LaCode);
            string laName = row.GetString(AllEstablishmentDataColumns.LaName);

            if (localAuthorities.TryGetValue(laCode, out LocalAuthority? existingLocalAuthority))
            {
                if (existingLocalAuthority.Name != laName)
                {
                    throw row.CreateException(
                        $"LA code {laCode} has more than one name ('{existingLocalAuthority.Name}' and '{laName}').");
                }
            }
            else
            {
                localAuthorities.Add(laCode, new LocalAuthority { LaCode = laCode, Name = laName });
            }

            Establishment establishment = new()
            {
                Urn = row.GetInt(AllEstablishmentDataColumns.Urn),
                LaCode = laCode,
                EstablishmentNumber = row.GetNullableInt(AllEstablishmentDataColumns.EstablishmentNumber),
                Name = row.GetString(AllEstablishmentDataColumns.EstablishmentName),
                Uprn = row.GetNullableLong(AllEstablishmentDataColumns.Uprn),
                TypeOfEstablishmentCode = row.GetInt(AllEstablishmentDataColumns.TypeOfEstablishmentCode),
                TypeOfEstablishmentName = row.GetString(AllEstablishmentDataColumns.TypeOfEstablishmentName),
                EstablishmentStatusCode = row.GetInt(AllEstablishmentDataColumns.EstablishmentStatusCode),
                EstablishmentStatusName = row.GetString(AllEstablishmentDataColumns.EstablishmentStatusName),
                CloseDate = row.GetNullableDate(AllEstablishmentDataColumns.CloseDate),
                PhaseOfEducationCode = row.GetInt(AllEstablishmentDataColumns.PhaseOfEducationCode),
                PhaseOfEducationName = row.GetString(AllEstablishmentDataColumns.PhaseOfEducationName),
                StatutoryLowAge = row.GetNullableInt(AllEstablishmentDataColumns.StatutoryLowAge),
                StatutoryHighAge = row.GetNullableInt(AllEstablishmentDataColumns.StatutoryHighAge),
                GorCode = row.GetNullableString(AllEstablishmentDataColumns.GorCode)
            };

            if (!establishments.TryAdd(establishment.Urn, establishment))
            {
                throw row.CreateException($"URN {establishment.Urn} appears more than once.");
            }
        }

        if (establishments.Count == 0)
        {
            throw new GiasExtractException(GiasExtractType.AllEstablishmentData, "The extract has no rows.");
        }

        return new GiasEstablishmentsExtract([.. localAuthorities.Values], [.. establishments.Values]);
    }

    public static GiasGroupLinksExtract ParseAllGroupLinks(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        Dictionary<int, EstablishmentGroup> groups = [];
        Dictionary<(int GroupUid, int Urn), EstablishmentGroupLink> links = [];

        foreach (GiasCsvRow row in ReadRows(stream, GiasExtractType.AllGroupLinks, AllGroupLinksColumns.All))
        {
            // Group details are repeated on every link row
            EstablishmentGroup group = new()
            {
                GroupUid = row.GetInt(AllGroupLinksColumns.GroupUid),
                GroupId = row.GetNullableString(AllGroupLinksColumns.GroupId),
                Name = row.GetString(AllGroupLinksColumns.GroupName),
                GroupTypeCode = row.GetInt(AllGroupLinksColumns.GroupTypeCode),
                GroupTypeName = row.GetString(AllGroupLinksColumns.GroupTypeName),
                GroupStatusCode = row.GetString(AllGroupLinksColumns.GroupStatusCode),
                GroupStatusName = row.GetString(AllGroupLinksColumns.GroupStatusName),
                ClosedDate = row.GetNullableDate(AllGroupLinksColumns.ClosedDate)
            };

            if (groups.TryGetValue(group.GroupUid, out EstablishmentGroup? existingGroup))
            {
                if (!HaveSameDetails(existingGroup, group))
                {
                    throw row.CreateException($"Group UID {group.GroupUid} has different details on different rows.");
                }
            }
            else
            {
                groups.Add(group.GroupUid, group);
            }

            EstablishmentGroupLink link = new()
            {
                GroupUid = group.GroupUid,
                Urn = row.GetInt(AllGroupLinksColumns.Urn),
                JoinedDate = row.GetNullableDate(AllGroupLinksColumns.JoinedDate)
            };

            if (!links.TryAdd((link.GroupUid, link.Urn), link))
            {
                throw row.CreateException($"URN {link.Urn} is linked to group UID {link.GroupUid} more than once.");
            }
        }

        if (links.Count == 0)
        {
            throw new GiasExtractException(GiasExtractType.AllGroupLinks, "The extract has no rows.");
        }

        return new GiasGroupLinksExtract([.. groups.Values], [.. links.Values]);
    }

    private static bool HaveSameDetails(EstablishmentGroup a, EstablishmentGroup b) =>
        a.GroupId == b.GroupId &&
        a.Name == b.Name &&
        a.GroupTypeCode == b.GroupTypeCode &&
        a.GroupTypeName == b.GroupTypeName &&
        a.GroupStatusCode == b.GroupStatusCode &&
        a.GroupStatusName == b.GroupStatusName &&
        a.ClosedDate == b.ClosedDate;

    private static IEnumerable<GiasCsvRow> ReadRows(Stream stream, GiasExtractType extractType, IReadOnlyList<string> requiredColumns)
    {
        using StreamReader streamReader = new(stream, _encoding);
        using CsvReader csvReader = new(streamReader, CultureInfo.InvariantCulture);

        if (!csvReader.Read())
        {
            throw new GiasExtractException(extractType, "The extract is empty.");
        }

        csvReader.ReadHeader();
        string[] header = csvReader.HeaderRecord!;

        string[] missingColumns = [.. requiredColumns.Except(header)];
        if (missingColumns.Length > 0)
        {
            throw new GiasExtractException(
                extractType,
                $"The extract is missing the following columns: {string.Join(", ", missingColumns.Select(c => $"'{c}'"))}.");
        }

        while (csvReader.Read())
        {
            yield return new GiasCsvRow(csvReader, extractType);
        }
    }

    private readonly struct GiasCsvRow(CsvReader csvReader, GiasExtractType extractType)
    {
        public string GetString(string column) =>
            GetNullableString(column) ?? throw CreateException($"'{column}' is empty.");

        public string? GetNullableString(string column)
        {
            string value = csvReader.GetField(column)!.Trim();
            return value.Length == 0 ? null : value;
        }

        public int GetInt(string column) => GetNullableInt(column) ?? throw CreateException($"'{column}' is empty.");

        public int? GetNullableInt(string column) =>
            GetNullableString(column) is { } value
                ? int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out int result)
                    ? result
                    : throw CreateException($"'{column}' is not a valid integer: '{value}'.")
                : null;

        public long? GetNullableLong(string column) =>
            GetNullableString(column) is { } value
                ? long.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out long result)
                    ? result
                    : throw CreateException($"'{column}' is not a valid integer: '{value}'.")
                : null;

        public DateOnly? GetNullableDate(string column) =>
            GetNullableString(column) is { } value
                ? DateOnly.TryParseExact(value, _dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly result)
                    ? result
                    : throw CreateException($"'{column}' is not a valid date: '{value}'.")
                : null;

        public GiasExtractException CreateException(string message) =>
            new(extractType, $"Row {csvReader.Parser.Row}: {message}");
    }

    internal static class AllEstablishmentDataColumns
    {
        public const string Urn = "URN";
        public const string LaCode = "LA (code)";
        public const string LaName = "LA (name)";
        public const string EstablishmentNumber = "EstablishmentNumber";
        public const string EstablishmentName = "EstablishmentName";
        public const string Uprn = "UPRN";
        public const string TypeOfEstablishmentCode = "TypeOfEstablishment (code)";
        public const string TypeOfEstablishmentName = "TypeOfEstablishment (name)";
        public const string EstablishmentStatusCode = "EstablishmentStatus (code)";
        public const string EstablishmentStatusName = "EstablishmentStatus (name)";
        public const string CloseDate = "CloseDate";
        public const string PhaseOfEducationCode = "PhaseOfEducation (code)";
        public const string PhaseOfEducationName = "PhaseOfEducation (name)";
        public const string StatutoryLowAge = "StatutoryLowAge";
        public const string StatutoryHighAge = "StatutoryHighAge";
        public const string GorCode = "GOR (code)";

        public static readonly IReadOnlyList<string> All =
        [
            Urn, LaCode, LaName, EstablishmentNumber, EstablishmentName, Uprn, TypeOfEstablishmentCode, TypeOfEstablishmentName,
            EstablishmentStatusCode, EstablishmentStatusName, CloseDate, PhaseOfEducationCode, PhaseOfEducationName,
            StatutoryLowAge, StatutoryHighAge, GorCode
        ];
    }

    internal static class AllGroupLinksColumns
    {
        public const string Urn = "URN";
        public const string GroupUid = "Group UID";
        public const string GroupId = "Group ID";
        public const string GroupName = "Group Name";
        public const string GroupTypeCode = "Group Type (code)";
        public const string GroupTypeName = "Group Type";
        public const string ClosedDate = "Closed Date";
        public const string GroupStatusCode = "Group Status (code)";
        public const string GroupStatusName = "Group Status";
        public const string JoinedDate = "Joined date";

        public static readonly IReadOnlyList<string> All =
        [
            Urn, GroupUid, GroupId, GroupName, GroupTypeCode, GroupTypeName, ClosedDate, GroupStatusCode, GroupStatusName, JoinedDate
        ];
    }
}

public sealed record GiasEstablishmentsExtract(
    IReadOnlyCollection<LocalAuthority> LocalAuthorities,
    IReadOnlyCollection<Establishment> Establishments);

public sealed record GiasGroupLinksExtract(
    IReadOnlyCollection<EstablishmentGroup> Groups,
    IReadOnlyCollection<EstablishmentGroupLink> Links);
