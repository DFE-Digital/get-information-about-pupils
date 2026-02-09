using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Dfe.Data.Common.Infrastructure.CognitiveSearch;
using Dfe.Data.Common.Infrastructure.CognitiveSearch.SearchByKeyword;
using Dfe.Data.Common.Infrastructure.CognitiveSearch.SearchByKeyword.Options;
using Dfe.Data.Common.Infrastructure.CognitiveSearch.SearchByKeyword.Providers;
using DfE.GIAP.Core.MyPupils;
using DfE.GIAP.Core.MyPupils.Application.Options;
using DfE.GIAP.Core.MyPupils.Application.Repositories;
using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.AddPupilsToMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.Core.MyPupils.Domain;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.DataTransferObjects;
using DfE.GIAP.Core.Search;
using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.Models;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.Models;
using DfE.GIAP.Core.Search.Extensions;
using DfE.GIAP.Core.Search.Infrastructure.Shared.Mappers;
using DfE.GIAP.SharedTests.Runtime;
using DfE.GIAP.SharedTests.Runtime.TestDoubles;
using DfE.GIAP.Web.Features.Search.Options.Search;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using AzureFacetResult = Azure.Search.Documents.Models.FacetResult;
using CompositionRoot = DfE.GIAP.Core.MyPupils.CompositionRoot;
using SearchOptions = DfE.GIAP.Web.Features.Search.Options.Search.SearchOptions;

namespace DfE.GIAP.Core.UnitTests.MyPupils;
public sealed class CompositionRootTests
{
    [Fact]
    public void ThrowsArgumentNullException_When_ServicesIsNull()
    {
        IServiceCollection? serviceCollection = null;
        Action register = () => CompositionRoot.AddMyPupilsCore(serviceCollection!);
        Assert.Throws<ArgumentNullException>(register);
    }

    [Fact]
    public void Registers_CompositionRoot_CanResolve_Services()
    {
        // Arrange
        IConfiguration configuration =
            ConfigurationTestDoubles.DefaultConfigurationBuilder()
                .WithAzureSearchConnectionOptions()
                .WithSearchOptions()
                .WithFilterKeyToFilterExpressionMapOptions()
                .Build();

        IServiceCollection services =
            ServiceCollectionTestDoubles.Default()
                .AddAspNetCoreRuntimeProvidedServices(configuration)
                .AddFeaturesSharedServices();

        // TODO TEMP while the dependency on AzureSearch for MyPupils exists

        services.AddSingleton<ISearchIndexOptionsProvider, SearchIndexOptionsProvider>();

        services
            .AddOptions<SearchOptions>()
            .Bind(configuration.GetSection(nameof(SearchOptions)));

        services.AddSingleton<SearchOptions>(sp => sp.GetRequiredService<IOptions<SearchOptions>>().Value);

        services
            .AddOptions<AzureSearchConnectionOptions>()
            .Bind(configuration.GetSection(nameof(AzureSearchConnectionOptions)));
        // END TODO TEMP

        services.AddSearchCore(configuration);
        services.RemoveAll<ISearchIndexNamesProvider>();
        services.AddSingleton<ISearchIndexNamesProvider, FakeSearchIndexNamesProvider>();
        services
            .AddNationalPupilDatabaseSearch()
            .AddPupilPremiumSearch()
            .AddMyPupilsCore();

        // Act
        IServiceProvider provider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(provider);

        Assert.NotNull(provider.GetService<IOptions<MyPupilsOptions>>());

        Assert.NotNull(provider.GetService<IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>>());
        Assert.NotNull(provider.GetService<IMapper<Pupil, MyPupilsModel>>());

        Assert.NotNull(provider.GetService<IUseCaseRequestOnly<AddPupilsToMyPupilsRequest>>());
        Assert.NotNull(provider.GetService<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>>());

        Assert.NotNull(provider.GetService<IAggregatePupilsForMyPupilsApplicationService>());
        Assert.NotNull(provider.GetService<IMapper<PupilPremiumLearner, Pupil>>());
        Assert.NotNull(provider.GetService<IMapper<NationalPupilDatabaseLearner, Pupil>>());


        Assert.NotNull(provider.GetService<IMyPupilsReadOnlyRepository>());
        Assert.NotNull(provider.GetService<IMapper<MyPupilsAggregate, MyPupilsDocumentDto>>());

        Assert.NotNull(provider.GetService<IMyPupilsWriteOnlyRepository>());

        Assert.NotNull(provider.GetService<IEnumerable<SearchClient>>());
    }

    private sealed class FakeSearchIndexNamesProvider : ISearchIndexNamesProvider
    {
        public IEnumerable<string> GetIndexNames() => [];
    }

}
