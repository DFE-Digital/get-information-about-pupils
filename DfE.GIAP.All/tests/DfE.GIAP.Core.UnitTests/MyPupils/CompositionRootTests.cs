using DfE.GIAP.Core.Common.Application.ValueObjects;
using DfE.GIAP.Core.MyPupils;
using DfE.GIAP.Core.MyPupils.Application.Options;
using DfE.GIAP.Core.MyPupils.Application.Ports;
using DfE.GIAP.Core.MyPupils.Application.Repositories;
using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.AddPupilsToMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.Core.MyPupils.Domain;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.DataTransferObjects;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.Models;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.Models;
using DfE.GIAP.SharedTests.Runtime;
using DfE.GIAP.SharedTests.Runtime.TestDoubles;
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
                .Build();

        IServiceCollection services =
            ServiceCollectionTestDoubles.Default()
                .AddAspNetCoreRuntimeProvidedServices(configuration)
                .AddFeaturesSharedServices()
                .AddMyPupilsCoreFakeAdaptors();

        services.AddMyPupilsCore();

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

        Assert.NotNull(provider.GetService<IMapper<IEnumerable<string>, UniquePupilNumbers>>());

        Assert.NotNull(provider.GetService<IMyPupilsReadOnlyRepository>());
        Assert.NotNull(provider.GetService<IMapper<MyPupilsAggregate, MyPupilsDocumentDto>>());

        Assert.NotNull(provider.GetService<IMyPupilsWriteOnlyRepository>());

        Assert.NotNull(provider.GetService<IQueryMyPupilsPort>());
        Assert.NotNull(provider.GetService<IMapper<PupilPremiumLearner, Pupil>>());
        Assert.NotNull(provider.GetService<IMapper<NationalPupilDatabaseLearner, Pupil>>());
    }

}

