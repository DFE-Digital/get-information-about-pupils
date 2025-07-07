namespace DfE.GIAP.Common.Constants;

public static class SecurityReportsConstants
{
    public const string DfeUserLocalAuthorityCode = "001";

    //Error messages
    public const string SecurityReportsByUpnEmptyUpnOrUln = "Enter a UPN or ULN";
    public const string SecurityReportsInvalidUpn = "A UPN must be 13 characters in the format as A123456789012 or A12345678901A";
    public const string SecurityReportsInvalidUln = "A ULN must be 10 digits in the format 1234567890";
    public const string SecurityReportsHTMLBRTag = "<br>";
    public const string SecurityReportsNoOrBothLocalAuthorityOrAcademyTrustSelected = "Please select either local authority or academy trust";
    public const string SecurityReportsNoOrganisationSelected = "Please select an organisation";
    public const string NoEstablishmentSelected = "A School must be selected";
}
