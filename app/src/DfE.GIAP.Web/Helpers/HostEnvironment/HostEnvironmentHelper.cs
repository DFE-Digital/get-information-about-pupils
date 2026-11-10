namespace DfE.GIAP.Web.Helpers.HostEnvironment;

public static class HostEnvironmentHelper
{
    public static bool IsK8s(this IHostEnvironment hostEnvironment)
    {
        return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("KUBERNETES_SERVICE_PORT"));
    }

    public static bool IsLocal(this IHostEnvironment hostEnvironment)
    {
        return hostEnvironment.IsEnvironment("Local") || hostEnvironment.IsEnvironment("local");
    }

    public static bool ShouldShowErrors()
    {
        return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development"
           || Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Test"
           || Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Local"
           || Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "local";
    }
}
