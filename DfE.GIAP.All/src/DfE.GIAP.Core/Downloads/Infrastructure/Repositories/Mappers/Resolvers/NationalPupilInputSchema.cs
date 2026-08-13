using DfE.GIAP.Core.Downloads.Application.Models.Entries;

namespace DfE.GIAP.Core.Downloads.Infrastructure.Repositories.Mappers.Resolvers;

public static class NationalPupilInputSchema
{
    public static readonly List<FieldMappingDefinition> Fields = new()
    {
        new() {
            SourceNames = { "UPN", "upn", "U_P_N" },
            TargetProperty = "Upn",
            TargetType = typeof(string)
        },

        new() {
            SourceNames = { "id", "_id", "pupilId" },
            TargetProperty = "Id",
            TargetType = typeof(string)
        },

        new() {
            SourceNames = { "PupilMatchingRef", "PMR", "Pupil_Matching_Ref" },
            TargetProperty = "PupilMatchingRef",
            TargetType = typeof(string)
        },

        new() {
            SourceNames = { "LA", "LocalAuthority", "LA_Code" },
            TargetProperty = "LA",
            TargetType = typeof(int)
        },

        new() {
            SourceNames = { "Estab", "ESTAB", "Establishment" },
            TargetProperty = "Estab",
            TargetType = typeof(int)
        },

        new() {
            SourceNames = { "URN", "Urn", "SchoolURN" },
            TargetProperty = "Urn",
            TargetType = typeof(int)
        },

        new() {
            SourceNames = { "Surname", "LastName", "FamilyName" },
            TargetProperty = "Surname",
            TargetType = typeof(string)
        },

        new() {
            SourceNames = { "Forename", "FirstName", "GivenName" },
            TargetProperty = "Forename",
            TargetType = typeof(string)
        },

        new() {
            SourceNames = { "MiddleName", "Middlenames", "Middle_Names" },
            TargetProperty = "MiddleName",
            TargetType = typeof(string)
        },

        new() {
            SourceNames = { "Gender", "GENDER", "SexCode" },
            TargetProperty = "Gender",
            TargetType = typeof(char?)
        },

        new() {
            SourceNames = { "Sex", "SEX", "GenderText" },
            TargetProperty = "Sex",
            TargetType = typeof(string)
        },

        new() {
            SourceNames = { "DOB", "DateOfBirth", "BirthDate", "DoB" },
            TargetProperty = "DOB",
            TargetType = typeof(DateTime)
        },

        // Nested dataset mapping (collection)
        new() {
            SourceNames = { "MTC", "Mtc", "mtc", "MultiplicationCheck" },
            TargetProperty = "MTC",
            TargetType = typeof(List<MtcEntry>)
        }
    };
}
