using DfE.GIAP.Core.Search.Application.Adapters;
using DfE.GIAP.Core.Search.Application.Models.Learner;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.UnitTests.Search.Infrastructure.TestDoubles;
using DfE.GIAP.Core.UnitTests.Search.TestHarness;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DfE.GIAP.Core.UnitTests.Search;

public class CompositionRootTests : IClassFixture<ConfigBuilder>, IClassFixture<CompositionRootServiceProvider>
{
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;

    private static Dictionary<string, string?> InMemoryConfig =>
        new()
        {
            {
                "AzureSearchOptions:Indexes:further-education:SearchIndex",
                "idx-further-education-v3"
            }
        };

    public CompositionRootTests(ConfigBuilder configBuilder, CompositionRootServiceProvider serviceProvider)
    {
        _configuration = configBuilder.SetupConfiguration(InMemoryConfig);
        _serviceProvider = serviceProvider.SetUpServiceProvider(_configuration);
    }

    [Fact]
    public async Task AddCognitiveSearchAdaptorServices_RegistersEverythingNeeded()
    {
        // arrange
        ISearchServiceAdapter<Learners, SearchFacets> adapter =
            _serviceProvider.GetRequiredService<ISearchServiceAdapter<Learners, SearchFacets>>();

        SearchServiceAdapterRequest searchServiceAdapterRequest =
            SearchServiceAdapterRequestTestDouble.Stub();

        // act
        SearchResults<Learners, SearchFacets> response =
            await adapter.SearchAsync(searchServiceAdapterRequest);

        // assert
        response.Should().NotBeNull();
    }
}
