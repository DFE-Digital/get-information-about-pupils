using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Content;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticles;
using DfE.GIAP.Core.NewsArticles.Infrastructure.Repositories;
using DfE.GIAP.Core.SharedTests.TestDoubles;
using DfE.GIAP.Core.SharedTests;
using Microsoft.Extensions.DependencyInjection;
using CompositionRoot = DfE.GIAP.Core.Content.CompositionRoot;
using DfE.GIAP.Core.Content.Application.UseCases.GetContentByPageKeyUseCase;
using DfE.GIAP.Core.Content.Infrastructure.Repositories;
using DfE.GIAP.Core.Content.Application.Model;
using DfE.GIAP.Core.Content.Application.Repository;
using Microsoft.Extensions.Options;
using DfE.GIAP.Core.Content.Application.Options;
using DfE.GIAP.Core.Content.Infrastructure.Repositories.Options;

namespace DfE.GIAP.Core.UnitTests.Content;
public sealed class CompositionRootTests
{
    [Fact]
    public void ThrowsArgumentNullException_When_ServicesIsNull()
    {
        IServiceCollection? serviceCollection = null;
        ConfigurationTestDoubles.Default();
        Action register = () => Core.Content.CompositionRoot.AddContentDependencies(serviceCollection);
        Assert.Throws<ArgumentNullException>(register);
    }

    [Fact]
    public void Registers_CompositionRoot_CanResolve_Services()
    {
        // Arrange
        IServiceCollection services = ServiceCollectionTestDoubles.Default().AddTestDependencies();

        // Act
        IServiceCollection registeredServices = CompositionRoot.AddContentDependencies(services);
        IServiceProvider provider = registeredServices.BuildServiceProvider();

        // Assert
        Assert.NotNull(registeredServices);
        Assert.NotNull(provider);

        Assert.NotNull(provider.GetService<IUseCase<GetContentByPageKeyUseCaseRequest, GetContentByPageKeyUseCaseResponse>>());
        Assert.NotNull(provider.GetService<IMapper<ContentDTO, Core.Content.Application.Model.Content>>());

        Assert.NotNull(provider.GetService<IContentReadOnlyRepository>());
        
        Assert.NotNull(provider.GetService<IOptions<PageContentOptions>>());
        Assert.NotNull(provider.GetService<IOptions<ContentRepositoryOptions>>());
        
    }
}
