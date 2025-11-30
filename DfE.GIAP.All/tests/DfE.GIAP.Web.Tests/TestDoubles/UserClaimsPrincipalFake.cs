using DfE.GIAP.Web.Features.Auth.Application.Claims;
using System.Security.Claims;

namespace DfE.GIAP.Web.Tests.TestDoubles
{
    public class UserClaimsPrincipalFake
    {
        public ClaimsPrincipal GetUserClaimsPrincipal()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Email, "discoverytest842@gmail.com"),
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim("custom-claim", "example claim value"),
                new Claim(AuthClaimTypes.UserId, "00000000-0000-0000-0000-000000000000"),
                new Claim(AuthClaimTypes.SessionId, "8cd6f4a2-85d7-4001-b53c-d7bb40b0ab67"),
                new Claim(ClaimTypes.Email,"test@yahoo.com"),
                new Claim(ClaimTypes.GivenName,"Abc"),
                new Claim(ClaimTypes.Surname,"xyz"),
                new Claim(AuthClaimTypes.OrganisationId, "123"),
                new Claim(AuthClaimTypes.OrganisationName, "Test Org"),
                new Claim(AuthClaimTypes.OrganisationCategoryId,"001"),
                new Claim(AuthClaimTypes.OrganisationEstablishmentTypeId, "00"),
                new Claim(AuthClaimTypes.OrganisationHighAge, "13"),
                new Claim(AuthClaimTypes.OrganisationLowAge, "2"),
                new Claim(AuthClaimTypes.EstablishmentNumber,"89"),
                new Claim(AuthClaimTypes.LocalAuthorityNumber,"98"),
                new Claim(AuthClaimTypes.UniqueReferenceNumber,"121"),
                new Claim(AuthClaimTypes.UniqueIdentifier,"007"),
                new Claim(AuthClaimTypes.UKProviderReferenceNumber, "23432")
            }, "mock"));

            return user;
        }

        public ClaimsPrincipal GetLAApproverClaimsPrincipal()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Email, "discoverytest842+LAApprover@gmail.com"),
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(AuthClaimTypes.UserId, "00000000-0000-0000-0000-000000000000"),
                new Claim(AuthClaimTypes.SessionId, "8cd6f4a2-85d7-4001-b53c-d7bb40b0ab67"),
                new Claim(ClaimTypes.Email,"test@yahoo.com"),
                new Claim(ClaimTypes.GivenName,"Abc"),
                new Claim(ClaimTypes.Surname,"xyz"),
                new Claim(AuthClaimTypes.OrganisationId, "123"),
                new Claim(AuthClaimTypes.OrganisationName, "Test Org"),
                new Claim(AuthClaimTypes.OrganisationCategoryId,"002"),
                new Claim(AuthClaimTypes.OrganisationHighAge, "20"),
                new Claim(AuthClaimTypes.OrganisationLowAge, "2"),
                new Claim(AuthClaimTypes.EstablishmentNumber,"89"),
                new Claim(AuthClaimTypes.LocalAuthorityNumber,"98"),
                new Claim(AuthClaimTypes.UniqueReferenceNumber,"121"),
                new Claim(AuthClaimTypes.UniqueIdentifier,"007"),
                new Claim(AuthClaimTypes.UKProviderReferenceNumber, "23432"),
                new Claim(ClaimTypes.Role,"GIAPApprover")
            }, "mock"));

            return user;
        }

        public ClaimsPrincipal GetSATApproverClaimsPrincipal()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Email, "discoverytest842+SATApprover@gmail.com"),
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim("custom-claim", "example claim value"),
                new Claim(AuthClaimTypes.UserId, "00000000-0000-0000-0000-000000000000"),
                new Claim(AuthClaimTypes.SessionId, "8cd6f4a2-85d7-4001-b53c-d7bb40b0ab67"),
                new Claim(ClaimTypes.Email,"test@yahoo.com"),
                new Claim(ClaimTypes.GivenName,"Abc"),
                new Claim(ClaimTypes.Surname,"xyz"),
                new Claim(AuthClaimTypes.OrganisationId, "123"),
                new Claim(AuthClaimTypes.OrganisationName, "Test Org"),
                new Claim(AuthClaimTypes.OrganisationCategoryId,"013"),
                new Claim(AuthClaimTypes.OrganisationHighAge, "20"),
                new Claim(AuthClaimTypes.OrganisationLowAge, "2"),
                new Claim(AuthClaimTypes.EstablishmentNumber,"89"),
                new Claim(AuthClaimTypes.LocalAuthorityNumber,"98"),
                new Claim(AuthClaimTypes.UniqueReferenceNumber,"121"),
                new Claim(AuthClaimTypes.UniqueIdentifier,"007"),
                new Claim(AuthClaimTypes.UKProviderReferenceNumber, "23432"),
                new Claim(ClaimTypes.Role,"GIAPApprover")
            }, "mock"));

            return user;
        }

        public ClaimsPrincipal GetAdminUserClaimsPrincipal()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Email,"discoverytest842+ADMIN@gmail.com"),
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim("custom-claim", "example claim value"),
                new Claim(AuthClaimTypes.UserId, "00000000-0000-0000-0000-000000000000"),
                new Claim(AuthClaimTypes.SessionId, "8cd6f4a2-85d7-4001-b53c-d7bb40b0ab67"),
                new Claim(ClaimTypes.Email,"test@yahoo.com"),
                new Claim(ClaimTypes.GivenName,"Abc"),
                new Claim(ClaimTypes.Surname,"xyz"),
                new Claim(AuthClaimTypes.OrganisationId, "123"),
                new Claim(AuthClaimTypes.OrganisationName, "Department for Education"),
                new Claim(AuthClaimTypes.OrganisationCategoryId,"789"),
                new Claim(AuthClaimTypes.OrganisationHighAge, "20"),
                new Claim(AuthClaimTypes.OrganisationLowAge, "2"),
                new Claim(AuthClaimTypes.EstablishmentNumber,"89"),
                new Claim(AuthClaimTypes.LocalAuthorityNumber,"98"),
                new Claim(AuthClaimTypes.UniqueReferenceNumber,"121"),
                new Claim(AuthClaimTypes.UniqueIdentifier,"007"),
                new Claim(AuthClaimTypes.UKProviderReferenceNumber, "23432"),
                new Claim(ClaimTypes.Role,"GIAPAdmin")
            }, "mock"));

            return user;
        }

        public ClaimsPrincipal GetFEApproverClaimsPrincipal()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Email,"discoverytest842+FEApprover@gmail.com"),
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(AuthClaimTypes.UserId, "00000000-0000-0000-0000-000000000000"),
                new Claim(AuthClaimTypes.SessionId, "8cd6f4a2-85d7-4001-b53c-d7bb40b0ab67"),
                new Claim(ClaimTypes.Email,"test@yahoo.com"),
                new Claim(ClaimTypes.GivenName,"Abc"),
                new Claim(ClaimTypes.Surname,"xyz"),
                new Claim(AuthClaimTypes.OrganisationId, "123"),
                new Claim(AuthClaimTypes.OrganisationName, "Test Org"),
                new Claim(AuthClaimTypes.OrganisationCategoryId, "001"),
                new Claim(AuthClaimTypes.OrganisationEstablishmentTypeId, "18"),
                new Claim(AuthClaimTypes.OrganisationHighAge, "20"),
                new Claim(AuthClaimTypes.OrganisationLowAge, "2"),
                new Claim(AuthClaimTypes.EstablishmentNumber,"89"),
                new Claim(AuthClaimTypes.LocalAuthorityNumber,"98"),
                new Claim(AuthClaimTypes.UniqueReferenceNumber,"121"),
                new Claim(AuthClaimTypes.UniqueIdentifier,"007"),
                new Claim(AuthClaimTypes.UKProviderReferenceNumber, "23432"),
                new Claim(ClaimTypes.Role,"GIAPApprover")
            }, "mock"));

            return user;
        }

        public ClaimsPrincipal GetLAUserClaimsPrincipal()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Email,"discoverytest842+LAApprover@gmail.com"),
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(AuthClaimTypes.UserId, "00000000-0000-0000-0000-000000000000"),
                new Claim(AuthClaimTypes.SessionId, "8cd6f4a2-85d7-4001-b53c-d7bb40b0ab67"),
                new Claim(ClaimTypes.Email,"test@yahoo.com"),
                new Claim(ClaimTypes.GivenName,"Abc"),
                new Claim(ClaimTypes.Surname,"xyz"),
                new Claim(AuthClaimTypes.OrganisationId, "123"),
                new Claim(AuthClaimTypes.OrganisationName, "Test Org"),
                new Claim(AuthClaimTypes.OrganisationCategoryId, "002"),
                new Claim(AuthClaimTypes.OrganisationEstablishmentTypeId, "18"),
                new Claim(AuthClaimTypes.OrganisationHighAge, "20"),
                new Claim(AuthClaimTypes.OrganisationLowAge, "2"),
                new Claim(AuthClaimTypes.EstablishmentNumber,"89"),
                new Claim(AuthClaimTypes.LocalAuthorityNumber,"98"),
                new Claim(AuthClaimTypes.UniqueReferenceNumber,"121"),
                new Claim(AuthClaimTypes.UniqueIdentifier,"007"),
                new Claim(AuthClaimTypes.UKProviderReferenceNumber, "23432"),
                new Claim(ClaimTypes.Role,"GIAPApprover")
            }, "mock"));

            return user;
        }

        public ClaimsPrincipal GetSpecificUserClaimsPrincipal(
            string organisationCategoryId,
            string organisationEstablishmentType,
            string role,
            int organisationLowAge,
            int organisationHighAge,
            string email = "testy.mctester@somewhere.net")
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(AuthClaimTypes.UserId, "00000000-0000-0000-0000-000000000000"),
                new Claim(AuthClaimTypes.SessionId, "8cd6f4a2-85d7-4001-b53c-d7bb40b0ab67"),
                new Claim(ClaimTypes.Email,"test@yahoo.com"),
                new Claim(ClaimTypes.GivenName,"Abc"),
                new Claim(ClaimTypes.Surname,"xyz"),
                new Claim(AuthClaimTypes.OrganisationId, "123"),
                new Claim(AuthClaimTypes.OrganisationName, "Test Org"),
                new Claim(AuthClaimTypes.OrganisationCategoryId, organisationCategoryId),
                new Claim(AuthClaimTypes.OrganisationEstablishmentTypeId, organisationEstablishmentType),
                new Claim(AuthClaimTypes.OrganisationHighAge, organisationHighAge.ToString()),
                new Claim(AuthClaimTypes.OrganisationLowAge, organisationLowAge.ToString()),
                new Claim(AuthClaimTypes.EstablishmentNumber,"89"),
                new Claim(AuthClaimTypes.LocalAuthorityNumber,"98"),
                new Claim(AuthClaimTypes.UniqueReferenceNumber,"121"),
                new Claim(AuthClaimTypes.UniqueIdentifier,"007"),
                new Claim(AuthClaimTypes.UKProviderReferenceNumber, "23432"),
                new Claim(ClaimTypes.Role, role)
            }, "mock"));

            return user;
        }
    }
}
