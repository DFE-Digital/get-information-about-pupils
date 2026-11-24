using DfE.GIAP.Verification.Anonymisation;

namespace DfE.GIAP.Verification.Tests.Anonymisation;

public sealed class ValueAnonymiserTests : IDisposable
{
    internal static readonly byte[] Key = [.. Enumerable.Range(0, 32).Select(i => (byte)(i * 7))];

    internal static readonly NameDictionary Names = new(
        ["Adam", "Ben", "Carl", "Dan"],
        ["Amy", "Beth", "Cara", "Dina"],
        ["Smith", "Jones", "Brown", "Patel", "Khan"]);

    internal static readonly IReadOnlyList<PmrFormat> PmrFormats =
    [
        new(IdentifierFormat.Parse("[0-9A-F]{18}"), IdentifierFormat.Parse("[G-V][0-9A-F]{17}")),
        new(IdentifierFormat.Parse("[A-Z][0-9]{12}"), IdentifierFormat.Parse("[A-Z]{2}[0-9]{11}"))
    ];

    private readonly ValueAnonymiser _anonymiser = new(Key, PmrFormats, Names);

    public void Dispose() => _anonymiser.Dispose();

    [Theory]
    [InlineData("CCA656CA36DABEFFC9", "[G-V][0-9A-F]{17}")]
    [InlineData("M891313215015", "[A-Z]{2}[0-9]{11}")]
    public void Anonymise_Pmr_IsInTheReservedRange(string pmr, string range)
    {
        string fake = Anonymise(ColumnTransform.Pmr, pmr);

        Assert.True(IdentifierFormat.Parse(range).Matches(fake), fake);
    }

    [Fact]
    public void Anonymise_PmrInNoFormat_Throws_WithoutTheValue()
    {
        AnonymisationException exception = Assert.Throws<AnonymisationException>(() => Anonymise(ColumnTransform.Pmr, "PMR1"));

        Assert.DoesNotContain("PMR1", exception.Message, StringComparison.Ordinal);
        Assert.Contains("AAA9", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Anonymise_ManyIdentifiers_NeverGivesTwoTheSameFakeValue()
    {
        string[] pmrs = [.. Enumerable.Range(0, 5000).Select(i => $"A{i:D12}")];
        string[] ulns = [.. Enumerable.Range(100_000_000, 20_000).Select(i => i.ToString()).Where(p => CheckCharacters.UlnCheckDigit(p) is not null).Select(p => p + CheckCharacters.UlnCheckDigit(p))];
        string[] upns = [.. Enumerable.Range(0, 5000).Select(i => $"{i:D12}").Select(d => CheckCharacters.UpnCheckLetter(d) + d)];

        Assert.Equal(pmrs.Length, pmrs.Select(v => Anonymise(ColumnTransform.Pmr, v)).Distinct().Count());
        Assert.Equal(ulns.Length, ulns.Select(v => Anonymise(ColumnTransform.Uln, v)).Distinct().Count());
        Assert.Equal(upns.Length, upns.Select(v => Anonymise(ColumnTransform.Upn, v)).Distinct().Count());
    }

    [Theory]
    [InlineData("P000000000001")]
    [InlineData("P00000000000B")]
    public void Anonymise_Upn_IsAnotherValidUpnOfTheSameKind(string upn)
    {
        string fake = Anonymise(ColumnTransform.Upn, upn);

        Assert.NotEqual(upn, fake);
        Assert.True(CheckCharacters.IsValidUpn(fake), fake);
        Assert.Equal(char.IsAsciiDigit(upn[12]), char.IsAsciiDigit(fake[12]));
    }

    [Theory]
    [InlineData("A000000000001")]
    [InlineData("UPN1")]
    [InlineData("P0000000000O1")]
    public void Anonymise_InvalidUpn_IsGarbled_AndStillInvalid(string upn)
    {
        string fake = Anonymise(ColumnTransform.Upn, upn);

        Assert.NotEqual(upn, fake);
        Assert.Equal(ValueAnonymiser.Shape(upn), ValueAnonymiser.Shape(fake));
        Assert.False(CheckCharacters.IsValidUpn(fake), fake);
    }

    [Fact]
    public void Anonymise_Uln_IsAnotherValidUln()
    {
        string fake = Anonymise(ColumnTransform.Uln, "1234567899");

        Assert.NotEqual("1234567899", fake);
        Assert.True(CheckCharacters.IsValidUln(fake), fake);
    }

    [Theory]
    [InlineData("1234567898")]
    [InlineData("723456789")]
    [InlineData("X234567899")]
    public void Anonymise_InvalidUln_IsGarbled_AndStillInvalid(string uln)
    {
        string fake = Anonymise(ColumnTransform.Uln, uln);

        Assert.NotEqual(uln, fake);
        Assert.Equal(ValueAnonymiser.Shape(uln), ValueAnonymiser.Shape(fake));
        Assert.False(CheckCharacters.IsValidUln(fake), fake);
    }

    [Fact]
    public void Anonymise_IsTheSameForTheSameKey()
    {
        using ValueAnonymiser other = new([.. Key], PmrFormats, Names);

        Assert.Equal(Anonymise(ColumnTransform.Uln, "1234567899"), other.Anonymise(ColumnTransform.Uln, "1234567899", null, Sex.Unknown));
        Assert.Equal(Anonymise(ColumnTransform.Surname, "Gilby", "P1"), other.Anonymise(ColumnTransform.Surname, "Gilby", "P1", Sex.Unknown));
    }

    [Fact]
    public void Anonymise_CandidateNumber_KeepsItsLength()
    {
        string fake = Anonymise(ColumnTransform.CandidateNumber, "006948");

        Assert.Matches("^[0-9]{6}$", fake);
        Assert.NotEqual("006948", fake);
    }

    [Theory]
    [InlineData("F0A62344-21AF-4258-9B3C-156F1B23B1F6", "^[0-9A-F]{8}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{12}$")]
    [InlineData("{f0a6234421af42589b3c156f1b23b1f6}", "^\\{[0-9a-f]{32}\\}$")]
    public void Anonymise_Guid_KeepsItsLayoutAndCase(string guid, string pattern)
    {
        string fake = Anonymise(ColumnTransform.Guid, guid);

        Assert.Matches(pattern, fake);
        Assert.NotEqual(guid, fake);
    }

    [Theory]
    [InlineData("Gilby", "^(Smith|Jones|Brown|Patel|Khan)$")]
    [InlineData("GILBY", "^(SMITH|JONES|BROWN|PATEL|KHAN)$")]
    [InlineData("Smith-Jones", "^[A-Z][a-z]+-[A-Z][a-z]+$")]
    [InlineData("de la Cruz", "^[a-z]+ [a-z]+ [A-Z][a-z]+$")]
    public void Anonymise_Surname_ReplacesEachWord_KeepingItsCaseAndSeparators(string surname, string pattern) =>
        Assert.Matches(pattern, Anonymise(ColumnTransform.Surname, surname, "P1"));

    [Fact]
    public void Anonymise_Name_IsNeverTheRealName()
    {
        foreach (string surname in Names.Surnames)
        {
            foreach (string subject in Enumerable.Range(0, 20).Select(i => $"P{i}"))
            {
                Assert.NotEqual(surname, Anonymise(ColumnTransform.Surname, surname, subject), StringComparer.OrdinalIgnoreCase);
            }
        }
    }

    [Fact]
    public void Anonymise_Name_IsTheSameForTheSamePupil_ButNotForEveryPupilWithIt()
    {
        Assert.Equal(Anonymise(ColumnTransform.Surname, "Gilby", "P1"), Anonymise(ColumnTransform.Surname, "Gilby", "P1"));

        string[] fakes = [.. Enumerable.Range(0, 50).Select(i => Anonymise(ColumnTransform.Surname, "Gilby", $"P{i}")).Distinct()];
        Assert.True(fakes.Length > 1);
    }

    [Theory]
    [InlineData(Sex.Male, "^(Adam|Ben|Carl|Dan)$")]
    [InlineData(Sex.Female, "^(Amy|Beth|Cara|Dina)$")]
    public void Anonymise_Forename_IsChosenFromTheirSex(Sex sex, string pattern) =>
        Assert.Matches(pattern, _anonymiser.Anonymise(ColumnTransform.Forename, "Skylar", "P1", sex));

    [Theory]
    [InlineData("20/10/2008", @"^(1[3-9]|2[0-9]|3[01])/10/2008$")]
    [InlineData("05/02/2015", @"^(0[1-9]|1[0-2])/02/2015$")]
    [InlineData("5/2/2015", @"^([1-9]|1[0-2])/2/2015$")]
    [InlineData("2008-10-20", @"^2008-10-(1[3-9]|2[0-9]|3[01])$")]
    [InlineData("2008-02-20T00:00:00", @"^2008-02-(1[3-9]|2[0-9])T00:00:00$")]
    public void Anonymise_Dob_MovesToAnotherDayInTheSameMonth_InTheSameFormat(string dob, string pattern)
    {
        string fake = Anonymise(ColumnTransform.Dob, dob, "P1");

        Assert.Matches(pattern, fake);
        Assert.NotEqual(dob, fake);
    }

    [Theory]
    [InlineData("31/02/2014")]
    [InlineData("00:00.0")]
    public void Anonymise_DobThatIsntADate_IsLeftAsItIs(string dob) =>
        Assert.Equal(dob, Anonymise(ColumnTransform.Dob, dob, "P1"));

    [Theory]
    [InlineData("SW1A 1AA", "SW1A")]
    [InlineData("m11ae", "m1")]
    [InlineData("B33", "B33")]
    [InlineData("Not a postcode", "")]
    public void Anonymise_Postcode_IsCutToItsOutwardCode(string postcode, string expected) =>
        Assert.Equal(expected, Anonymise(ColumnTransform.Postcode, postcode));

    [Theory]
    [InlineData(ColumnTransform.Address)]
    [InlineData(ColumnTransform.Blank)]
    public void Anonymise_AddressOrBlank_IsEmpty(ColumnTransform transform) =>
        Assert.Equal(string.Empty, Anonymise(transform, "1 High Street"));

    private string Anonymise(ColumnTransform transform, string value, string? subject = null) =>
        _anonymiser.Anonymise(transform, value, subject, Sex.Unknown);
}
