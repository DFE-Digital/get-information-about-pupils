using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Providers.Session;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace DfE.GIAP.Web.IntegrationTests.WebApplicationFactory;

internal sealed class GetInformationAboutPupilsWebApplicationFactory : WebApplicationFactory<Program>
{
    public GetInformationAboutPupilsWebApplicationFactory()
    {

        ISessionProvider sessionProvider = Services.GetRequiredService<ISessionProvider>();
        sessionProvider.SetSessionValue(SessionKeys.ConsentKey, SessionKeys.ConsentValue);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Development);
        Environment.SetEnvironmentVariable("AZURE_APP_CONFIGURATION_PROVIDER_DISABLED", "true");

        builder.UseUrls("https://localhost");

        IConfiguration configuration = ConfigurationTestDoubles.GetTestConfiguration();

        builder.UseConfiguration(configuration);

        builder.ConfigureTestServices(
            (testServices) =>
            {
                // TODO Note current issue is precedence, RepositoryOptions are overriden from default appsettings.json as being the last registered thing, re-register them
                testServices.RemoveAll<IConfiguration>();
                testServices.AddSingleton(configuration);

                // Required as AddAzureAppConfiguration calls to refresh configuration
                testServices.TryAddSingleton<IConfigurationRefresherProvider, StubConfigurationRefresherProvider>();

                // ConsentMiddleware reads from SessionProvider to determine if Consent has been given
                testServices.RemoveAll<ISessionProvider>();
                testServices.AddSingleton<ISessionProvider, StubSessionProvider>();

                // Bypassing DSI, ideally I setup an OpenId
                // TODO what if I need to configure the ClaimsPrincipal somehow? 

                testServices.AddAuthentication((options) =>
                {
                    options.DefaultSignInScheme = TestAuthHandler.TestScheme;
                    options.DefaultAuthenticateScheme = TestAuthHandler.TestScheme;
                    options.DefaultChallengeScheme = TestAuthHandler.TestScheme;
                }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.TestScheme, (options) =>
                {

                });
            });
    }
}
