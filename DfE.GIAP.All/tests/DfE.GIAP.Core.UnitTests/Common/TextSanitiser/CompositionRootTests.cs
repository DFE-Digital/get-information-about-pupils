using DfE.GIAP.Core.Common;
using DfE.GIAP.Core.Common.Application.TextSanitiser.Abstraction.Handler;
using DfE.GIAP.Core.SharedTests;
using DfE.GIAP.Core.SharedTests.TestDoubles;
using Microsoft.Extensions.DependencyInjection;

namespace DfE.GIAP.Core.UnitTests.Common.TextSanitiser;
public sealed class CompositionRootTests
{
    [Fact]
    public void Registers_CompositionRoot_CanResolve_Services()
    {
        // Arrange
        IServiceCollection services = ServiceCollectionTestDoubles.Default()
            .AddSharedTestDependencies()
            .AddFeaturesSharedDependencies();

        // Act
        IServiceProvider provider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(provider);
        Assert.NotNull(provider.GetService<ITextSanitiserInvoker>());
    }
}
