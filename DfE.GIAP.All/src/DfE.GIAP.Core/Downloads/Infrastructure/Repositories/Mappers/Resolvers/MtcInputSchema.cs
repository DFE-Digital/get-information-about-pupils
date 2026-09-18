namespace DfE.GIAP.Core.Downloads.Infrastructure.Repositories.Mappers.Resolvers;

public static class MtcInputSchema
{
    public static readonly List<FieldMappingDefinition> Fields = new()
    {
        new() {
            SourceNames = { "ACADYR" },
            TargetProperty = "ACADYR",
            TargetType = typeof(string)
        },

        new() {
            SourceNames = { "PupilMatchingRef" },
            TargetProperty = "PupilMatchingRef",
            TargetType = typeof(string)
        },

        new() {
            SourceNames = { "UPN", "upn" },
            TargetProperty = "UPN",
            TargetType = typeof(string)
        },

        new() {
            SourceNames = { "Surname" },
            TargetProperty = "Surname",
            TargetType = typeof(string)
        },

        new() {
            SourceNames = { "Forename" },
            TargetProperty = "Forename",
            TargetType = typeof(string)
        },

        new() {
            SourceNames = { "Sex" },
            TargetProperty = "Sex",
            TargetType = typeof(string)
        },

        new() {
            SourceNames = { "DOB", "DateOfBirth" },
            TargetProperty = "DOB",
            TargetType = typeof(DateTime?)
        },

        new() {
            SourceNames = { "LA" },
            TargetProperty = "LA",
            TargetType = typeof(string)
        },

        new() {
            SourceNames = { "LA_9Code" },
            TargetProperty = "LA_9Code",
            TargetType = typeof(string)
        },

        new() {
            SourceNames = { "ESTAB", "Estab" },
            TargetProperty = "Estab",
            TargetType = typeof(string)
        },

        new() {
            SourceNames = { "LAestab", "LAEstab" },
            TargetProperty = "LAEstab",
            TargetType = typeof(string)
        },

        new() {
            SourceNames = { "URN" },
            TargetProperty = "URN",
            TargetType = typeof(string)
        },

        new() {
            SourceNames = { "ToECode", "ToE_Code" },
            TargetProperty = "ToECode",
            TargetType = typeof(string)
        },

        new() {
            SourceNames = { "FormMark" },
            TargetProperty = "FormMark",
            TargetType = typeof(string)
        },

        new() {
            SourceNames = { "PupilStatus" },
            TargetProperty = "PupilStatus",
            TargetType = typeof(string)
        },

        new() {
            SourceNames = { "ReasonNotTakingCheck" },
            TargetProperty = "ReasonNotTakingCheck",
            TargetType = typeof(string)
        }
    };
}

