using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Domain.Models.SecurityReports;
using System.Collections.Generic;

namespace DfE.GIAP.Service.Tests.FakeData
{
    public class SecurityServiceResultsFake
    {
        public AzureAppSettings GetAppSettings()
        {
            var appSettings = new AzureAppSettings()
            {
                QueryLAGetAllUrl = "https://www.somewhere1.com",
                QueryLAByCodeUrl = "https://www.somewhere2.com",
                GetAcademiesURL = "https://www.somewhere3.com",
                GetAllFurtherEducationURL = "https://www.somewhere4.com",
                GetFurtherEducationByCodeURL = "https://www.somewhere5.com"
            };

            return appSettings;
        }

        public List<LocalAuthority> GetListOfAllLocalAuthorities()
        {
            var list = new List<LocalAuthority>();
            list.Add(new LocalAuthority() { Name = "Test LA Name 1", Code = 001 });

            return list;
        }

        public List<AcademyTrust> GetListOfAllAcademyTrusts()
        {
            var list = new List<AcademyTrust>();
            list.Add(new AcademyTrust() { Name = "Test AT Name 1", Code = "001" });

            return list;
        }

        public EstablishmentResponse GetEstablishmentResponse()
        {
            var list = new List<Establishment>();
            list.Add(new Establishment() { Name = "Test Establishment Name 1", URN = "001" });

            var establishmentResponse = new EstablishmentResponse() { Establishments = list };

            return establishmentResponse;
        }

        public List<Establishment> GetAcademyEstablishments()
        {
            var establishments = new List<Establishment>();
            establishments.Add(new Establishment() { Name = "Test Establishment Name 1", URN = "001" });

            return establishments;
        }

        public List<AcademyTrust> GetEstablishmentsByAcademyTrustCode(List<string> docTypes, string academyTrustCode)
        {
            var establishments = new List<Establishment>();
            establishments.Add(new Establishment() { Name = "Test Establishment Name 1", URN = "001" });
            var academyTrusts = new List<AcademyTrust>();
            academyTrusts.Add(new AcademyTrust() { Name = "Test AT Name 1", Code = "001", Establishments = establishments });

            return academyTrusts;
        }
    }
}
