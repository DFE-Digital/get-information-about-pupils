namespace DfE.GIAP.Web.Constants;

public static class DsiKeys
{
    public static class Scope
    {
        public const string OpenId = "openid";
        public const string Email = "email";
        public const string Profile = "profile";
        public const string OrganisationId = "organisationid";
    }

    public static class CallbackPaths
    {
        public const string Dsi = "/auth/cb";
        public const string SignedOut = "/signout/complete";
        public const string Logout = "/auth/logout";
    }

    public static class Common
    {
        //NOTE: DfE user is set up as a LA with 001 as the LA number.
        public const string DepartmentId = "001";
        public const string DepartmentForEducation = "Department for Education";
    }

    public static class EstablishmentType
    {
        public const string CommunitySchool = "001";
        public const string FurtherEducation = "18";
        public const string FurtherEducationString = "018";
    }

    public static class OrganisationCategory
    {
        public const string Establishment = "001";
        public const string LocalAuthority = "002";
        public const string MultiAcademyTrust = "010";
        public const string SingleAcademyTrust = "013";
        public const string FurtherEducation = "051";
    }
}
