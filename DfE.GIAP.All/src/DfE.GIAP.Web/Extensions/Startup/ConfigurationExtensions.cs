namespace DfE.GIAP.Web.Extensions.Startup;

public static class ConfigurationExtensions
{
    public static IConfigurationBuilder ConfigureSettings(this IConfigurationBuilder builder)
    {
        string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        builder
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        return builder;
    }
}
