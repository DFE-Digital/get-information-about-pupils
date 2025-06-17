using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Contents.Application.Models;
using DfE.GIAP.Core.Contents.Application.Options;
using DfE.GIAP.Core.Contents.Application.Repositories;
using DfE.GIAP.Core.Contents.Application.UseCases.GetContentByPageKeyUseCase;
using DfE.GIAP.Core.Contents.Infrastructure.Repositories;
using DfE.GIAP.Core.SharedTests;
using DfE.GIAP.Core.SharedTests.TestDoubles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using CompositionRoot = DfE.GIAP.Core.Contents.CompositionRoot;

namespace DfE.GIAP.Core.UnitTests.Contents;
public sealed class CompositionRootTests
{
    [Fact]
    public void ThrowsArgumentNullException_When_ServicesIsNull()
    {
        IServiceCollection? serviceCollection = null;
        ConfigurationTestDoubles.Default();
        Action register = () => CompositionRoot.AddContentDependencies(serviceCollection!);
        Assert.Throws<ArgumentNullException>(register);
    }

    [Fact]
    public void Registers_CompositionRoot_CanResolve_Services()
    {
        // Arrange
        IServiceCollection services = ServiceCollectionTestDoubles.Default().AddSharedDependencies();

        // Act
        IServiceCollection registeredServices = CompositionRoot.AddContentDependencies(services);
        IServiceProvider provider = registeredServices.BuildServiceProvider();

        // Assert
        Assert.NotNull(registeredServices);
        Assert.NotNull(provider);

        Assert.NotNull(provider.GetService<IUseCase<GetContentByPageKeyUseCaseRequest, GetContentByPageKeyUseCaseResponse>>());
        Assert.NotNull(provider.GetService<IMapper<ContentDto, Content>>());
        Assert.NotNull(provider.GetService<IContentReadOnlyRepository>());
        Assert.NotNull(provider.GetService<IOptions<PageContentOptions>>());
    }
}
