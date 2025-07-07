namespace DfE.GIAP.Web.Constants;

public static class AppSettings
{
    public const string DsiScopeOpenId = "openid";
    public const string DsiScopeEmail = "email";
    public const string DsiScopeProfile = "profile";
    public const string DsiScopeOrganisationId = "organisationid";

    public const string DsiCallbackPath = "/auth/cb";
    public const string DsiSignedOutCallbackPath = "/signout/complete";
    public const string DsiLogoutPath = "/auth/logout";
}
