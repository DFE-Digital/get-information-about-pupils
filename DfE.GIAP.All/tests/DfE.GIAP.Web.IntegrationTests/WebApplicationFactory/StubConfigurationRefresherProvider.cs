using Microsoft.Extensions.Configuration.AzureAppConfiguration;

namespace DfE.GIAP.Web.IntegrationTests.WebApplicationFactory;
public sealed class StubConfigurationRefresherProvider : IConfigurationRefresherProvider
{
    public IEnumerable<IConfigurationRefresher> Refreshers => [];
}
