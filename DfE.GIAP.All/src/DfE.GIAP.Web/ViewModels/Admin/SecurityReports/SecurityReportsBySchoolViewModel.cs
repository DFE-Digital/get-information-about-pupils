using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Web.ViewModels.Admin.SecurityReports;

[ExcludeFromCodeCoverage]
public class SecurityReportsBySchoolViewModel : BaseSecurityReportsViewModel
{
    [Display(Name = "SelectedReportType")]
    [Required(ErrorMessage = "A report must be selected")]
    public string SelectedReportType { get; set; }
    public string SelectedReportTypeFormatted => GetFormattedReportType(SelectedReportType);
    public string SelectedOrganisationCode { get; set; }
    public string SelectedOrganisationCodeID => GetOrganisationCodeID(SelectedOrganisationCode);
    public string SelectedOrganisationCodeDocType => GetOrganisationCodeDocType(SelectedOrganisationCode);
    public string SelectedOrganisationType => GetOrganisationType(SelectedOrganisationCodeDocType);
    public string SelectedOrganisationOption { get; set; }
    public string SelectedEstablishmentCode { get; set; }
    public string SelectedEstablishmentName => GetEstablishmentName(SelectedEstablishmentCode);
    public string SelectedConfirmationOption { get; set; }
    public bool ProcessDownload { get; set; }
    public bool NoSRContent { get; set; }
    public IEnumerable<SelectListItem> ListOfSelectItems { get; set; }
    public BackButtonViewModel BackButton { get; set; }


    // Helper Methods
    private string GetFormattedReportType(string reportType)
    {
        return reportType switch
        {
            "logindetails" => "Log-in details",
            "detailedsearches" => "Detailed searches",
            _ => string.Empty
        };
    }

    private string GetOrganisationCodeID(string organisationCode)
    {
        return ExtractOrganisationCodePart(organisationCode, 0);
    }

    private string GetOrganisationCodeDocType(string organisationCode)
    {
        return ExtractOrganisationCodePart(organisationCode, 1);
    }

    private string GetOrganisationType(string docType)
    {
        return docType switch
        {
            "FE" => "Further education",
            "LA" => "Local authority",
            "MAT" or "SAT" => "Academy trust",
            _ => string.Empty
        };
    }

    private string GetEstablishmentName(string establishmentCode)
    {
        return ExtractOrganisationCodePart(establishmentCode, 1);
    }

    private string ExtractOrganisationCodePart(string organisationCode, int partIndex)
    {
        if (!string.IsNullOrEmpty(organisationCode) && organisationCode.Contains("|"))
        {
            string[] parts = organisationCode.Split('|');
            return parts.Length > partIndex ? parts[partIndex] : string.Empty;
        }
        return string.Empty;
    }
}


