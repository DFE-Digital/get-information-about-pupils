using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Domain.Models.LoggingEvent
{
    [ExcludeFromCodeCoverage]
    public class LoggingEvent
    {
        public string UserGuid { get; set; }
        public string UserEmail { get; set; }
        public string UserGivenName { get; set; }
        public string UserSurname { get; set; }
        public string UserIpAddress { get; set; }
        public string OrganisationGuid { get; set; }
        public string OrganisationName { get; set; }
        public string OrganisationCategoryID { get; set; }
        public string OrganisationType { get; set; }
        public string EstablishmentNumber { get; set; }
        public string LocalAuthorityNumber { get; set; }
        public string UniqueReferenceNumber { get; set; }
        public string UniqueIdentifier { get; set; }
        public string UKProviderReferenceNumber { get; set; }
        public string GIAPUserRole { get; set; }
        public string SessionId { get; set; }
        public string ActionName { get; set; }
        public string ActionDescription { get; set; }
        public string PrePreparedDownloadsFileName { get; set; }
        public string PrePreparedDownloadsFileUploadedDate { get; set; }
    }
}
