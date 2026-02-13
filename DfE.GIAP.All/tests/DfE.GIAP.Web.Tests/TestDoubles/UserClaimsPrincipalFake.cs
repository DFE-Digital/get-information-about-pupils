using System.Security.Claims;
using DfE.GIAP.Web.Features.Auth.Application.Claims;

namespace DfE.GIAP.Web.Tests.TestDoubles;

public static class UserClaimsPrincipalFake
{
    public static ClaimsPrincipal GetUserClaimsPrincipal()
    {
        ClaimsPrincipal user = new(new ClaimsIdentity(new Claim[]
        {
            new(ClaimTypes.Email, "discoverytest842@gmail.com"),
            new(ClaimTypes.NameIdentifier, "1"),
            new("custom-claim", "example claim value"),
            new(AuthClaimTypes.UserId, "00000000-0000-0000-0000-000000000000"),
            new(AuthClaimTypes.SessionId, "8cd6f4a2-85d7-4001-b53c-d7bb40b0ab67"),
            new(ClaimTypes.Email,"test@yahoo.com"),
            new(ClaimTypes.GivenName,"Abc"),
            new(ClaimTypes.Surname,"xyz"),
            new(AuthClaimTypes.OrganisationId, "123"),
            new(AuthClaimTypes.OrganisationName, "Test Org"),
            new(AuthClaimTypes.OrganisationCategoryId,"001"),
            new(AuthClaimTypes.OrganisationEstablishmentTypeId, "00"),
            new(AuthClaimTypes.OrganisationHighAge, "13"),
            new(AuthClaimTypes.OrganisationLowAge, "2"),
            new(AuthClaimTypes.EstablishmentNumber,"89"),
            new(AuthClaimTypes.LocalAuthorityNumber,"98"),
            new(AuthClaimTypes.UniqueReferenceNumber,"121"),
            new(AuthClaimTypes.UniqueIdentifier,"007"),
            new(AuthClaimTypes.UKProviderReferenceNumber, "23432")
        }, "mock"));

        return user;
    }

    public static ClaimsPrincipal GetAdminUserClaimsPrincipal()
    {
        ClaimsPrincipal user = new(new ClaimsIdentity(new Claim[]
        {
            new(ClaimTypes.Email,"discoverytest842+ADMIN@gmail.com"),
            new(ClaimTypes.NameIdentifier, "1"),
            new("custom-claim", "example claim value"),
            new(AuthClaimTypes.UserId, "00000000-0000-0000-0000-000000000000"),
            new(AuthClaimTypes.SessionId, "8cd6f4a2-85d7-4001-b53c-d7bb40b0ab67"),
            new(ClaimTypes.Email,"test@yahoo.com"),
            new(ClaimTypes.GivenName,"Abc"),
            new(ClaimTypes.Surname,"xyz"),
            new(AuthClaimTypes.OrganisationId, "123"),
            new(AuthClaimTypes.OrganisationName, "Department for Education"),
            new(AuthClaimTypes.OrganisationCategoryId,"789"),
            new(AuthClaimTypes.OrganisationHighAge, "20"),
            new(AuthClaimTypes.OrganisationLowAge, "2"),
            new(AuthClaimTypes.EstablishmentNumber,"89"),
            new(AuthClaimTypes.LocalAuthorityNumber,"98"),
            new(AuthClaimTypes.UniqueReferenceNumber,"121"),
            new(AuthClaimTypes.UniqueIdentifier,"007"),
            new(AuthClaimTypes.UKProviderReferenceNumber, "23432"),
            new(ClaimTypes.Role,"GIAPAdmin")
        }, "mock"));

        return user;
    }

    public static ClaimsPrincipal GetFEApproverClaimsPrincipal()
    {
        ClaimsPrincipal user = new(new ClaimsIdentity(new Claim[]
        {
            new(ClaimTypes.Email,"discoverytest842+FEApprover@gmail.com"),
            new(ClaimTypes.NameIdentifier, "1"),
            new(AuthClaimTypes.UserId, "00000000-0000-0000-0000-000000000000"),
            new(AuthClaimTypes.SessionId, "8cd6f4a2-85d7-4001-b53c-d7bb40b0ab67"),
            new(ClaimTypes.Email,"test@yahoo.com"),
            new(ClaimTypes.GivenName,"Abc"),
            new(ClaimTypes.Surname,"xyz"),
            new(AuthClaimTypes.OrganisationId, "123"),
            new(AuthClaimTypes.OrganisationName, "Test Org"),
            new(AuthClaimTypes.OrganisationCategoryId, "001"),
            new(AuthClaimTypes.OrganisationEstablishmentTypeId, "18"),
            new(AuthClaimTypes.OrganisationHighAge, "20"),
            new(AuthClaimTypes.OrganisationLowAge, "2"),
            new(AuthClaimTypes.EstablishmentNumber,"89"),
            new(AuthClaimTypes.LocalAuthorityNumber,"98"),
            new(AuthClaimTypes.UniqueReferenceNumber,"121"),
            new(AuthClaimTypes.UniqueIdentifier,"007"),
            new(AuthClaimTypes.UKProviderReferenceNumber, "23432"),
            new(ClaimTypes.Role,"GIAPApprover")
        }, "mock"));

        return user;
    }

    public static ClaimsPrincipal GetLAUserClaimsPrincipal()
    {
        ClaimsPrincipal user = new(new ClaimsIdentity(new Claim[]
        {
            new(ClaimTypes.Email,"discoverytest842+LAApprover@gmail.com"),
            new(ClaimTypes.NameIdentifier, "1"),
            new(AuthClaimTypes.UserId, "00000000-0000-0000-0000-000000000000"),
            new(AuthClaimTypes.SessionId, "8cd6f4a2-85d7-4001-b53c-d7bb40b0ab67"),
            new(ClaimTypes.Email,"test@yahoo.com"),
            new(ClaimTypes.GivenName,"Abc"),
            new(ClaimTypes.Surname,"xyz"),
            new(AuthClaimTypes.OrganisationId, "123"),
            new(AuthClaimTypes.OrganisationName, "Test Org"),
            new(AuthClaimTypes.OrganisationCategoryId, "002"),
            new(AuthClaimTypes.OrganisationEstablishmentTypeId, "18"),
            new(AuthClaimTypes.OrganisationHighAge, "20"),
            new(AuthClaimTypes.OrganisationLowAge, "2"),
            new(AuthClaimTypes.EstablishmentNumber,"89"),
            new(AuthClaimTypes.LocalAuthorityNumber,"98"),
            new(AuthClaimTypes.UniqueReferenceNumber,"121"),
            new(AuthClaimTypes.UniqueIdentifier,"007"),
            new(AuthClaimTypes.UKProviderReferenceNumber, "23432"),
            new(ClaimTypes.Role,"GIAPApprover")
        }, "mock"));

        return user;
    }

    public static ClaimsPrincipal GetSpecificUserClaimsPrincipal(
        string organisationCategoryId,
        string organisationEstablishmentType,
        string role,
        int organisationLowAge,
        int organisationHighAge,
        string email = "testy.mctester@somewhere.net")
    {
        ClaimsPrincipal user = new(new ClaimsIdentity(new Claim[]
        {
            new(ClaimTypes.Email, email),
            new(ClaimTypes.NameIdentifier, "1"),
            new(AuthClaimTypes.UserId, "00000000-0000-0000-0000-000000000000"),
            new(AuthClaimTypes.SessionId, "8cd6f4a2-85d7-4001-b53c-d7bb40b0ab67"),
            new(ClaimTypes.Email,"test@yahoo.com"),
            new(ClaimTypes.GivenName,"Abc"),
            new(ClaimTypes.Surname,"xyz"),
            new(AuthClaimTypes.OrganisationId, "123"),
            new(AuthClaimTypes.OrganisationName, "Test Org"),
            new(AuthClaimTypes.OrganisationCategoryId, organisationCategoryId),
            new(AuthClaimTypes.OrganisationEstablishmentTypeId, organisationEstablishmentType),
            new(AuthClaimTypes.OrganisationHighAge, organisationHighAge.ToString()),
            new(AuthClaimTypes.OrganisationLowAge, organisationLowAge.ToString()),
            new(AuthClaimTypes.EstablishmentNumber,"89"),
            new(AuthClaimTypes.LocalAuthorityNumber,"98"),
            new(AuthClaimTypes.UniqueReferenceNumber,"121"),
            new(AuthClaimTypes.UniqueIdentifier,"007"),
            new(AuthClaimTypes.UKProviderReferenceNumber, "23432"),
            new(ClaimTypes.Role, role)
        }, "mock"));

        return user;
    }
}
