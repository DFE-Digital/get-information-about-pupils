using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Domain.Models.User
{
    [ExcludeFromCodeCoverage]
    public static class CustomClaimTypes
    {
        public const string OrganisationId = "http://schemas.microsoft.com/ws/2008/06/identity/claims/organisationid";
        public const string OrganisationName = "http://schemas.microsoft.com/ws/2008/06/identity/claims/organisationname";
        public const string EstablishmentNumber = "http://schemas.microsoft.com/ws/2008/06/identity/claims/establishmentnumber";
        public const string LocalAuthorityNumber = "http://schemas.microsoft.com/ws/2008/06/identity/claims/localauthoritynumber";
        public const string UniqueReferenceNumber = "http://schemas.microsoft.com/ws/2008/06/identity/claims/uniquereferencenumber";
        public const string UniqueIdentifier = "http://schemas.microsoft.com/ws/2008/06/identity/claims/uniqueidentifier";
        public const string UKProviderReferenceNumber = "http://schemas.microsoft.com/ws/2008/06/identity/claims/ukproviderreferencenumber";

        public const string OrganisationCategoryId = "http://schemas.microsoft.com/ws/2008/06/identity/claims/organisationcategoryid";
        public const string OrganisationEstablishmentTypeId = "http://schemas.microsoft.com/ws/2008/06/identity/claims/organisationestablishmenttypeid";
        public const string OrganisationLowAge = "http://schemas.microsoft.com/ws/2008/06/identity/claims/organisationlowage";
        public const string OrganisationHighAge = "http://schemas.microsoft.com/ws/2008/06/identity/claims/organisationhighage";

        public const string UserId = "http://schemas.microsoft.com/ws/2008/06/identity/claims/userid";
        public const string SessionId = "http://schemas.microsoft.com/ws/2008/06/identity/claims/sessionid";

        public const string IsAdmin = "http://schemas.microsoft.com/ws/2008/06/identity/claims/isadmin";
        public const string IsApprover = "http://schemas.microsoft.com/ws/2008/06/identity/claims/isapprover";
        public const string IsUser = "http://schemas.microsoft.com/ws/2008/06/identity/claims/isuser";
    }
}
