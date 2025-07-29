using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils;
using DfE.GIAP.Core.SharedTests;
using DfE.GIAP.Core.SharedTests.TestDoubles;
using DfE.GIAP.Core.User.Application.Repository;
using DfE.GIAP.Core.User.Infrastructure.Repository.Dtos;
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

        Assert.NotNull(provider.GetService<IUserReadOnlyRepository>());
        Assert.NotNull(provider.GetService<IMapper<UserDto, User.Application.User>>());
    }
}
