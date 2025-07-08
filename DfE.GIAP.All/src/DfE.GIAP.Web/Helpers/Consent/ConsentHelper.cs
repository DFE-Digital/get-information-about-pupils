namespace DfE.GIAP.Web.Helpers.Consent;

public static class ConsentHelper
{
    public const string ConsentKey = "cg";
    public const string ConsentValue = "yes";

    public static bool HasGivenConsent(HttpContext context)
        => context.Session.Keys.Contains(ConsentKey) &&
            context.Session.GetString(ConsentKey).Equals(ConsentValue);

    public static void SetConsent(HttpContext context) => context.Session.SetString(ConsentKey, ConsentValue);

    public static void RemoveConsent(HttpContext context) => context.Session.Remove(ConsentKey);
}
