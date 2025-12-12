using Azure.Search.Documents;
using DfE.Data.ComponentLibrary.Infrastructure.Persistence.CosmosDb;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils;
using DfE.GIAP.Core.MyPupils.Application.Repositories;
using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils;
using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils.DataTransferObjects;
using DfE.GIAP.Core.MyPupils.Application.UseCases.AddPupilsToMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.DeleteAllPupilsFromMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.Core.MyPupils.Domain;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.DataTransferObjects;
using DfE.GIAP.Core.MyPupils.Infrastructure.Search;
using DfE.GIAP.Core.Search;
using DfE.GIAP.SharedTests;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using CompositionRoot = DfE.GIAP.Core.MyPupils.CompositionRoot;

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
            .WithSearchIndexOptions()
            .WithAzureSearchConnectionOptions()
            .Build();

        IServiceCollection services =
            ServiceCollectionTestDoubles.Default()
                .AddSharedApplicationServices()
                .AddSearchDependencies(configuration)
                .AddCosmosDbDependencies()
                .AddMyPupilsCore();

        // Act
        IServiceProvider provider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(provider);

        Assert.NotNull(provider.GetService<IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>>());
        Assert.NotNull(provider.GetService<IMapper<Pupil, MyPupilModel>>());

        Assert.NotNull(provider.GetService<IUseCaseRequestOnly<AddPupilsToMyPupilsRequest>>());
        Assert.NotNull(provider.GetService<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>>());
        Assert.NotNull(provider.GetService<IUseCaseRequestOnly<DeleteAllMyPupilsRequest>>());

        Assert.NotNull(provider.GetService<IAggregatePupilsForMyPupilsApplicationService>());
        Assert.NotNull(provider.GetService<IMapper<AzureIndexEntityWithPupilType, Pupil>>());
        

        Assert.NotNull(provider.GetService<IMyPupilsReadOnlyRepository>());
        Assert.NotNull(provider.GetService<IMapper<MyPupilsAggregate, MyPupilsDocumentDto>>());

        Assert.NotNull(provider.GetService<IMyPupilsWriteOnlyRepository>());

        Assert.NotNull(provider.GetService<ISearchClientProvider>());
        Assert.NotNull(provider.GetService<IEnumerable<SearchClient>>());
    }
}
