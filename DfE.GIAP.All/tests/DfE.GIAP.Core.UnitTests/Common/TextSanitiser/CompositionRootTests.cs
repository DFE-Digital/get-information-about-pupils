using DfE.GIAP.Core.Common;
using DfE.GIAP.Core.Common.Application.TextSanitiser.Invoker;
using DfE.GIAP.SharedTests.Runtime;
using DfE.GIAP.SharedTests.TestDoubles;
using Microsoft.Extensions.DependencyInjection;

namespace DfE.GIAP.Core.UnitTests.Common.TextSanitiser;
public sealed class CompositionRootTests
{
    [Fact]
    public void Registers_CompositionRoot_CanResolve_Services()
    {
        // Arrange
        IServiceCollection services = ServiceCollectionTestDoubles.Default()
            .AddAspNetCoreRuntimeProvidedServices()
            .AddFeaturesSharedDependencies();

        // Act
        IServiceProvider provider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(provider);
        Assert.NotNull(provider.GetService<ITextSanitiser>());
    }
}
