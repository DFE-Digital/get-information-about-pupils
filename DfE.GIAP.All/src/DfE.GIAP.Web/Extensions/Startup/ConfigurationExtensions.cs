using Microsoft.Extensions.Configuration;
using System;

namespace DfE.GIAP.Web.Extensions.Startup
{
    public static class ConfigurationExtensions
    {
        public static IConfigurationBuilder ConfigureSettings(this IConfigurationBuilder builder)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            builder
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            return builder;
        }
    }
}
