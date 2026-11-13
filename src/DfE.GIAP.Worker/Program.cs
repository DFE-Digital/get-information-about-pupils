using DfE.GIAP.Core.Common;
using DfE.GIAP.Core.Common.Infrastructure.Postgres;
using DfE.GIAP.Worker.Jobs;
using Hangfire;
using Hangfire.PostgreSql;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    // The Worker is published to a subdirectory of the web app's Docker image; load the Worker's
    // appsettings from its own directory rather than the web app's from the working directory.
    ContentRootPath = AppContext.BaseDirectory
});

if (builder.Environment.IsEnvironment("Local"))
{
    builder.Configuration.AddUserSecrets<Program>(optional: true, reloadOnChange: true);
}

if (builder.Environment.IsProduction())
{
    string? sentryDsn = builder.Configuration["Sentry:Dsn"];
    if (string.IsNullOrWhiteSpace(sentryDsn))
    {
        throw new InvalidOperationException("Sentry:Dsn must be configured in Production.");
    }

    builder.Logging.AddSentry(options => options.Dsn = sentryDsn);
}

string postgresConnectionString = builder.Configuration.GetConnectionString(GiapDbContext.ConnectionName) ??
    throw new InvalidOperationException($"Connection string '{GiapDbContext.ConnectionName}' not found.");

builder.Services.AddGiapDbContext(postgresConnectionString);

builder.Services
    .AddHangfire(configuration => configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UsePostgreSqlStorage(
            options => options.UseNpgsqlConnection(postgresConnectionString),
            new PostgreSqlStorageOptions
            {
                SchemaName = "hangfire",
                PrepareSchemaIfNecessary = true,
                // Keep a running job invisible to other servers for as long as it's running, rather than for
                // a fixed timeout, so a long-running job isn't picked up again by another replica.
                UseSlidingInvisibilityTimeout = true
            }))
    .AddHostedService<RecurringJobsRegistrar>()
    .AddHangfireServer();

IHost host = builder.Build();

await host.RunAsync();
