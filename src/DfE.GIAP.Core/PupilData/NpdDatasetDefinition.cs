using DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;

namespace DfE.GIAP.Core.PupilData;

/// <summary>
/// The columns of an NPD dataset's files that hold a pupil's identifying fields. Column names are matched
/// case-insensitively, and every one of them must be in the file.
/// </summary>
/// <remarks>
/// Ported from the old pipeline's <c>dictionary_mapping_to_master</c> and <c>dictionary_mapping_to_header</c>.
/// </remarks>
public sealed record NpdDatasetDefinition(
    NpdDataset Dataset,
    string PupilMatchingRef,
    string LaCode,
    string EstablishmentNumber,
    string Urn,
    string Upn,
    string Surname,
    string Forename,
    string? MiddleNames,
    string Sex,
    string Dob,
    string AcademicYear)
{
    public static IReadOnlyDictionary<IngestDataset, NpdDatasetDefinition> All { get; } = new Dictionary<IngestDataset, NpdDatasetDefinition>
    {
        [IngestDataset.CensusAutumn] = Census(NpdDataset.CensusAutumn),
        [IngestDataset.CensusSpring] = Census(NpdDataset.CensusSpring),
        [IngestDataset.CensusSummer] = Census(NpdDataset.CensusSummer),
        [IngestDataset.Eyfsp] = new(
            NpdDataset.Eyfsp,
            PupilMatchingRef: "FSP_PUPILMATCHINGREF",
            LaCode: "FSP_LA",
            EstablishmentNumber: "FSP_ESTAB",
            Urn: "FSP_URN",
            Upn: "FSP_UPN",
            Surname: "FSP_SURNAME",
            Forename: "FSP_FORENAME",
            MiddleNames: null,
            Sex: "FSP_SEX",
            Dob: "FSP_DOB",
            AcademicYear: "FSP_ACADYR"),
        [IngestDataset.Phonics] = new(
            NpdDataset.Phonics,
            PupilMatchingRef: "Phonics_PUPILMATCHINGREF",
            LaCode: "Phonics_LA",
            EstablishmentNumber: "Phonics_ESTAB",
            Urn: "Phonics_URN",
            Upn: "Phonics_UPN",
            Surname: "Phonics_SURNAME",
            Forename: "Phonics_FORENAMES",
            MiddleNames: null,
            Sex: "Phonics_SEX",
            Dob: "Phonics_DOB",
            AcademicYear: "Phonics_ACADYR"),
        [IngestDataset.Ks1] = KeyStage(NpdDataset.Ks1),
        [IngestDataset.Ks2] = KeyStage(NpdDataset.Ks2),
        [IngestDataset.Ks4] = new(
            NpdDataset.Ks4,
            PupilMatchingRef: "KS4_PUPILMATCHINGREF",
            LaCode: "KS4_LA",
            EstablishmentNumber: "KS4_ESTAB",
            Urn: "KS4_URN",
            Upn: "KS4_UPN",
            Surname: "KS4_SURNAME",
            Forename: "KS4_FORENAMES",
            MiddleNames: null,
            Sex: "KS4_SEX",
            Dob: "KS4_DOB",
            AcademicYear: "KS4_ACADYR"),
        [IngestDataset.Mtc] = new(
            NpdDataset.Mtc,
            PupilMatchingRef: "PupilMatchingRef",
            LaCode: "LA",
            EstablishmentNumber: "Estab",
            Urn: "URN",
            Upn: "UPN",
            Surname: "Surname",
            Forename: "Forename",
            MiddleNames: null,
            Sex: "Sex",
            Dob: "DOB",
            AcademicYear: "ACADYR")
    };

    public IEnumerable<string> Columns =>
        new[] { PupilMatchingRef, LaCode, EstablishmentNumber, Urn, Upn, Surname, Forename, MiddleNames, Sex, Dob, AcademicYear }
            .OfType<string>();

    private static NpdDatasetDefinition Census(NpdDataset dataset) =>
        new(
            dataset,
            PupilMatchingRef: "PupilMatchingRef",
            LaCode: "LA",
            EstablishmentNumber: "Estab",
            Urn: "URN",
            Upn: "UPN",
            Surname: "Surname",
            Forename: "Forename",
            MiddleNames: "MiddleNames",
            Sex: "Sex",
            Dob: "DOB",
            AcademicYear: "AcademicYear");

    private static NpdDatasetDefinition KeyStage(NpdDataset dataset) =>
        new(
            dataset,
            PupilMatchingRef: "PupilMatchingRef",
            LaCode: "LA",
            EstablishmentNumber: "ESTAB",
            Urn: "URN",
            Upn: "UPN",
            Surname: "SURNAME",
            Forename: "FORENAMES",
            MiddleNames: null,
            Sex: "SEX",
            Dob: "DOB",
            AcademicYear: "ACADYR");
}
