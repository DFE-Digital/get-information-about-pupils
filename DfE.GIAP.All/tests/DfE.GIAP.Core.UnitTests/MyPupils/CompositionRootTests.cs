using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.AuthorisationContext;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Repository;
using DfE.GIAP.Core.MyPupils.Domain.Aggregate;
using DfE.GIAP.Core.MyPupils.Domain.Authorisation;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Domain.Services;
using DfE.GIAP.Core.MyPupils.Infrastructure.Repository;
using DfE.GIAP.Core.SharedTests;
using DfE.GIAP.Core.SharedTests.TestDoubles;
using Microsoft.Extensions.DependencyInjection;
using CompositionRoot = DfE.GIAP.Core.MyPupils.CompositionRoot;

namespace DfE.GIAP.Core.UnitTests.MyPupils;
public sealed class CompositionRootTests
{
    [Fact]
    public void ThrowsArgumentNullException_When_ServicesIsNull()
    {
        IServiceCollection? serviceCollection = null;
        Action register = () => CompositionRoot.AddMyPupilsDependencies(serviceCollection!);
        Assert.Throws<ArgumentNullException>(register);
    }

    [Fact]
    public void Registers_CompositionRoot_CanResolve_Services()
    {
        // Arrange
        IServiceCollection services =
            ServiceCollectionTestDoubles.Default()
                .AddSharedTestDependencies()
                .AddMyPupilsDependencies();

        // Act
        IServiceProvider provider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(provider);

        Assert.NotNull(provider.GetService<IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>>());
        Assert.NotNull(provider.GetService<IMapper<IAuthorisationContext, PupilAuthorisationContext>>());
        Assert.NotNull(provider.GetService<IMapper<Pupil, PupilItemPresentationModel>>());

        Assert.NotNull(provider.GetService<IAggregatePupilsForMyPupilsDomainService>());

        Assert.NotNull(provider.GetService<IUserReadOnlyRepository>());
        Assert.NotNull(provider.GetService<IUserAggregateRootFactory>());
        Assert.NotNull(provider.GetService<IMapper<UserProfileDto, User>>());
    }
}
