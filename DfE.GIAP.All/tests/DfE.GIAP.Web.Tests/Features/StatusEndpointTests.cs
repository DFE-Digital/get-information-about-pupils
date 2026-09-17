using DfE.GIAP.SharedTests.Runtime.TestDoubles;
using DfE.GIAP.Web.Middleware;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace DfE.GIAP.Web.Tests.Features;

public class StatusEndpointTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private static readonly string[] s_dsiEnvironmentVariableNames =
    [
        "DsiOptions__CallbackPath",
        "DsiOptions__SignedOutCallbackPath",
        "DsiOptions__MetadataAddress",
    ];

    private readonly Dictionary<string, string?> _originalDsiEnvironmentVariables;
    private readonly WebApplicationFactory<Program> _factory;

    public StatusEndpointTests(WebApplicationFactory<Program> factory)
    {
        // appsettings.json contains placeholder values (e.g. "ENDPOINT_URI", "CALLBACK_PATH") that are
        // only ever replaced with real values during deployment. Locally these are typically overridden
        // via user secrets / environment variables, but CI has neither.
        //
        // DsiOptions is bound eagerly in Program.cs (AddAuthDependencies) BEFORE builder.Build() runs,
        // so WebApplicationFactory's ConfigureAppConfiguration customizations (which are only merged in
        // at the deferred Build() step) are applied too late to affect it. Environment variables are
        // read as part of the same ConfigureSettings() call in Program.cs, so setting them here - before
        // the host is actually built on first use - is picked up in time.
        //
        // These are process-wide, so we capture whatever was there beforehand and restore it in
        // Dispose(), rather than assuming it's safe to simply clear the variable afterwards.
        _originalDsiEnvironmentVariables = s_dsiEnvironmentVariableNames
            .ToDictionary(name => name, Environment.GetEnvironmentVariable);

        Environment.SetEnvironmentVariable("DsiOptions__CallbackPath", "/signin-oidc");
        Environment.SetEnvironmentVariable("DsiOptions__SignedOutCallbackPath", "/signout-callback-oidc");
        Environment.SetEnvironmentVariable("DsiOptions__MetadataAddress", "https://integrationtest.example/.well-known/openid-configuration");

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, configBuilder) =>
            {
                configBuilder
                    .WithAzureSearchConnectionOptions();
            });
        });
    }

    public void Dispose()
    {
        foreach (KeyValuePair<string, string?> original in _originalDsiEnvironmentVariables)
        {
            Environment.SetEnvironmentVariable(original.Key, original.Value);
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task Status_ReturnsSuccessStatusCode()
    {
        using HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/status");

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Status_ReturnsHealthyBody()
    {
        using HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/status");
        string content = await response.Content.ReadAsStringAsync();

        Assert.Equal("Healthy", content);
    }
}
