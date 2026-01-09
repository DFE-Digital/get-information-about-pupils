using System.Globalization;

namespace DfE.GIAP.Web.Features.MyPupils.PresentationService;

public record MyPupilsPresentationPupilModel
{
    public string UniquePupilNumber { get; init; }
    public string Forename { get; init; }
    public string Surname { get; init; }
    public string DateOfBirth { get; init; }
    public string PupilPremiumLabel { get; init; }
    public string Sex { get; init; }
    public string LocalAuthorityCode { get; init; }
    public bool IsSelected { get; internal set; }
    public DateTime ParseDateOfBirth()
        => DateTime.TryParse(DateOfBirth, new CultureInfo("en-GB"), out DateTime result) ?
                result :
                    DateTime.MinValue;
}
