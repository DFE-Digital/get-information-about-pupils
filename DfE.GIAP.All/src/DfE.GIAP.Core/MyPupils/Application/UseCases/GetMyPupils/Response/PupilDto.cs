using System.Globalization;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
public record PupilDto
{
    public required string UniquePupilNumber { get; init; }
    public required string Forename { get; init; }
    public required string Surname { get; init; }
    public required string DateOfBirth { get; init; }
    public required string Sex { get; init; }
    public required bool IsPupilPremium { get; init; }
    public required int LocalAuthorityCode { get; init; }

    public DateTime ParseDateOfBirth()
    {
        return DateTime.TryParse(
            DateOfBirth, new CultureInfo("en-GB"), out DateTime result) ? result : DateTime.MinValue;
    }
}
