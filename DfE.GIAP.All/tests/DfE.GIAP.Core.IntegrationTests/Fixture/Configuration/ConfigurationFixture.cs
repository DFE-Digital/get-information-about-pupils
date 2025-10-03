using Microsoft.Extensions.Configuration;

namespace DfE.GIAP.Core.IntegrationTests.Fixture.Configuration;

public class ConfigurationFixture
{
    public IConfiguration Configuration { get; }

    public ConfigurationFixture()
    {
        Dictionary<string, string?> stubSettings = new()
        {
            ["Search:IndexName"] = "further-education",
            ["Search:ApiKey"] = "SEFSOFOIWSJFSO",
            ["FeatureFlags:EnableMotifTracing"] = "true",
            // SearchCriteria
            ["SearchCriteria:SearchFields:0"] = "Forename",
            ["SearchCriteria:SearchFields:1"] = "Surname",
            ["SearchCriteria:Facets:0"] = "ForenameLC",
            ["SearchCriteria:Facets:1"] = "SurnameLC",
            ["SearchCriteria:Facets:2"] = "Gender",
            ["SearchCriteria:Facets:3"] = "Sex"
        };

        Configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(stubSettings)
            .Build();
    }
}
