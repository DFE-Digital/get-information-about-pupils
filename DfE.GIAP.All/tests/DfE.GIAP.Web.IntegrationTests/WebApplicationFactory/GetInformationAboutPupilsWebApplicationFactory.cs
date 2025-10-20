using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using DfE.GIAP.Domain.Models.User;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Providers.Session;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DfE.GIAP.Web.IntegrationTests.WebApplicationFactory;

internal sealed class GetInformationAboutPupilsWebApplicationFactory : WebApplicationFactory<Program>
{
    public GetInformationAboutPupilsWebApplicationFactory()
    {
        ISessionProvider sessionProvider = Services.GetRequiredService<ISessionProvider>();
        sessionProvider.SetSessionValue(SessionKeys.ConsentGivenKey, true);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Development);
        Environment.SetEnvironmentVariable("AZURE_APP_CONFIGURATION_PROVIDER_DISABLED", "true");

        builder.UseUrls("https://localhost");

        IConfiguration configuration = ConfigurationTestDoubles.DefaultConfigurationBuilder()
            .WithLocalCosmosDbOptions()
            .WithSearchIndexOptions()
            .WithStorageAccountOptions()
            .WithFeatureFlagsOptions()
            .WithDsiOptions()
            .Build();

        builder.UseConfiguration(configuration);

        builder.ConfigureTestServices(
            (testServices) =>
            {
                // TODO Note issue is precedence, RepositoryOptions are overriden from default appsettings.json as being the last registered thing, re-register them
                // TODO can I remove my side on the ConfigureWebHost or does it need splitting?
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
public sealed class StubConfigurationRefresherProvider : IConfigurationRefresherProvider
{
    public IEnumerable<IConfigurationRefresher> Refreshers => [];
}

public class StubSessionProvider : ISessionProvider
{
    private readonly Dictionary<string, string> _store = [];

    public void SetSessionValue(string key, string value) => _store[key] = value;

    public void SetSessionValue<T>(string key, T value) => _store[key] = JsonSerializer.Serialize(value);

    public string? GetSessionValue(string key) => _store.TryGetValue(key, out string? value) ? value : null;

    public T? GetSessionValueOrDefault<T>(string key) =>
        _store.TryGetValue(key, out string? value) ?
            JsonSerializer.Deserialize<T>(value) :
            default;

    public void RemoveSessionValue(string key) => _store.Remove(key);

    public bool ContainsSessionKey(string key) => _store.ContainsKey(key);

    public void ClearSession() => _store.Clear();
}

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string TestScheme = "TestScheme";

    public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        Claim[] claims =
        [
            new Claim(ClaimTypes.Name, "TestUser"),
            new Claim(ClaimTypes.Role, Roles.Admin),
            new Claim(CustomClaimTypes.UserId, "MY_DSI_ID"),
            // Add more claims as needed
        ];

        ClaimsIdentity identity = new(claims, "DfE-SignIn");
        ClaimsPrincipal principal = new(identity);
        AuthenticationTicket ticket = new(principal, "DfE-SignIn");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

[CollectionDefinition(Name)]
public sealed class WebIntegrationTestsCollectionMarker : ICollectionFixture<CosmosDbFixture>
{
    public const string Name = "WebIntegrationTests";
}
