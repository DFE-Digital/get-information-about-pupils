using DfE.GIAP.Domain.Models.SecurityReports;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DfE.GIAP.Service.Security
{
    public interface ISecurityService
    {
        Task<IList<LocalAuthority>> GetAllLocalAuthorities();
        Task<IList<AcademyTrust>> GetAcademyTrusts(List<string> docTypes, string id = null);
        Task<IList<LocalAuthority>> GetAllFurtherEducationOrganisations();
        //Task<IList<Establishment>> GetEstablishmentsByLocalAuthorityCode(string docType, string localAuthorityCode);
        Task<IList<Establishment>> GetEstablishmentsByAcademyTrustCode(List<string> docTypes, string academyTrustCode);
        Task<IList<Establishment>> GetEstablishmentsByOrganisationCode(string docType, string academyTrustCode);
    }
}
