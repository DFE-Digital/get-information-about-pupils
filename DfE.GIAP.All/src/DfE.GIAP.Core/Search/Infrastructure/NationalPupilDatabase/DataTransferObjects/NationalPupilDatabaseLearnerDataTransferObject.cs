namespace DfE.GIAP.Core.Search.Infrastructure.NationalPupilDatabase.DataTransferObjects;
public record NationalPupilDatabaseLearnerDataTransferObject
{
    public string? UPN { get; set; }
    public string? Forename { get; set; }

    public string? Surname { get; set; }

    public string? Middlenames { get; set; }

    public string? Sex { get; set; }

    public string? Gender { get; set; }

    public DateTime? DOB { get; set; }

    public string? LocalAuthority { get; set; }
}
